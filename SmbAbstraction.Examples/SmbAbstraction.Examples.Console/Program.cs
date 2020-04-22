using System;
using System.IO.Abstractions;
using SmbAbstraction;

namespace SmbAbstraction.Examples.Console
{
    class Program
    {
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
    }
}
