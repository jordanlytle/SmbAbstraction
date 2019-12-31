# SmbAbstraction

This library implements the System.IO.Abstractions interfaces for interacting
with the filesystem, and adds support for interacting with UNC or SMB paths.

The project is curretly a work in progress and is not guaranteed to work for
your specific application.

## Usage

If you are already using the System.IO.Abstractions interfaces and dependency 
injection, you should only have to do the these things to make it work:

- inject an ISMBClientFactory (implemented by SMBClientFactory)
- inject an ISMBCredentialProvider (implemented by SMBCredentialProvider)
- inject an ISMBFileSystem (implemented by SMBFileSystem) instead of the System.IO.Abstractions FileSystem
- wrap your existing filesystem calls with an SMBCredential. (example below)

### Example

```CSharp
using (var credential = new SMBCredential(domain, username, password, filePath, credentialProvider))
{
    // Your calls to the filesystem interface go here.
}
```

## Notes

Currently the maxBufferSize needs to be set to the default, which is 65536. The server *should* be using
the buffer size that it sends back to the client, but doesn't seem to be honoring that. 
