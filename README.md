# SmbAbstraction

This library implements the System.IO.Abstractions interfaces for interacting
with the filesystem, and adds support for interacting with UNC or SMB paths.

The project is curretly a work in progress and is not guaranteed to work for
your specific application.

# Usage

## Examples

Example projects are available to view in `SMBAbstractions.Examples`

## Dependency Injection

### Registering Services
```CSharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<ISMBClientFactory>(new SMB2ClientFactory());
            services.AddSingleton<ISMBCredentialProvider>(new SMBCredentialProvider());

                    
            var serviceCollection = services.BuildServiceProvider();

            var clientFactory = serviceCollection.GetRequiredService<ISMBClientFactory>();
            var credentialProvider = serviceCollection.GetRequiredService<ISMBCredentialProvider>();
            var loggerFactory = serviceCollection.GetRequiredService<ILoggerFactory>(); //optional

            services.AddSingleton<IFileSystem>(new SMBFileSystem(clientFactory, credentialProvider, 65536u, loggerFactory));
        });
```

### Making calls

#### With IDisposable 
```CSharp
public void FileOpsInContext()
{
    var path = "valid_unc/smb_path";
    using (var credential = new SMBCredential("domain", "username", "password", path, _credentialProvider))
    {
        //FileInfo
        //_fileSystem.FileInfo.FromFileName(path)

        //DirectoryInfo
        //_fileSystem.DirectoryInfo.FromDirectoryName(path)

        //Stream
        //using (var stream = _fileSystem.File.Open(path, System.IO.FileMode.Open))
        //{
                    
        //}
    }
}
```

#### With Stored Credentials 
You can add/cache credentials for a share so that you can operate them from raw IFileSystem calls
```CSharp
public void StoreCredentialsForShare(NetworkCredential credential)
{
    var domain = credential.Domain;
    var username = credential.UserName;
    var password = credential.Password;

    var sharePath = "valid_unc/smb_sharepath"; //ie. \\host\sharename or smb://host/sharename

    _credentialProvider.AddSMBCredential(new SMBCredential(domain, username, password, sharePath, _credentialProvider));
}

public void UseStoredCredentialsForFileOp()
{
    //FileInfo
    //_fileSystem.FileInfo.FromFileName(path)

    //DirectoryInfo
    //_fileSystem.DirectoryInfo.FromDirectoryName(path)

    //Stream
    //using (var stream = _fileSystem.File.Open(path, System.IO.FileMode.Open))
    //{

    //}
}
```

## Raw

```CSharp
 static void Main(string[] args)
{
    var domain = "domain";
    var username = "username";
    var password = "password";

    var sharePath = "valid_unc/smb_share_path"; //ie. \\host\sharename or smb://host/sharename

    ISMBCredentialProvider credentialProvider = new SMBCredentialProvider();
    ISMBClientFactory clientFactory = new SMB2ClientFactory();
    IFileSystem fileSystem = new SMBFileSystem(clientFactory, credentialProvider, 65536u);

    //var path = _fileSystem.Path.Combine(sharePath, "test.txt");

    using (var credential = new SMBCredential(domain, username, password, sharePath, credentialProvider)) // NOTE: You can interchange path with sharePath here. 
    {                                                                                                     // SMBCredential will parse the share path from path
        //FileInfo
        //_fileSystem.FileInfo.FromFileName(path)

        //DirectoryInfo
        //_fileSystem.DirectoryInfo.FromDirectoryName(path)

        //Stream
        //using (var stream = _fileSystem.File.Open(path, System.IO.FileMode.Open))
        //{

        //}
    }
}
```


# Notes

Currently the maxBufferSize needs to be set to the default, which is 65536. The server *should* be using
the buffer size that it sends back to the client, but doesn't seem to be honoring that. 
