using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net;
using System.Text;

namespace SmbAbstraction.Examples.DependencyInjection
{
    public class NetworkShareService
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        public NetworkShareService(IFileSystem fileSystem, ISMBCredentialProvider credentialProvider)
        {
            _fileSystem = fileSystem;
            _credentialProvider = credentialProvider;
        }

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

        //You can add/cache credentials for a share so that you can operate them from raw IFileSystem calls
        public void StoreCredentialsForShare(NetworkCredential credential)
        {
            var domain = credential.Domain;
            var username = credential.UserName;
            var password = credential.Password;

            var sharePath = "valid_unc/smb_sharepath"; //ie. \\host\sharename or smb://host/sharename

            _credentialProvider.AddSMBCredential(new SMBCredential(domain, username, password, sharePath, _credentialProvider));
        }

        public void StoreCredentialsForShare(SMBCredential credential)
        {
            _credentialProvider.AddSMBCredential(credential);
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
    }
}
