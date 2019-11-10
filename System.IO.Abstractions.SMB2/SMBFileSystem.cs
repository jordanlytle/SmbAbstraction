using System;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileSystem : IFileSystem
    {
        private string _loginDomainName;
        private string _loginUserName;
        private string _loginPassword;
        private ISMBCredentialProvider _credentialProvider;

        public SMBFileSystem(ISMBClient iSMBClient, ISMBCredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
            Directory = new SMBDirectory();
            File = new SMBFile(iSMBClient, credentialProvider);
            FileInfo = new SMBFileInfoFactory();
            FileStream = new SMBFileStreamFactory();
            Path = new PathWrapper(this);
            DirectoryInfo = new SMBDirectoryInfoFactory();
            DriveInfo = new SMBDriveInfoFactory();
        }

        public IDirectory Directory { get; }

        public IFile File { get; }

        public IFileInfoFactory FileInfo { get; }

        public IFileStreamFactory FileStream { get; }

        public IPath Path { get; }

        public IDirectoryInfoFactory DirectoryInfo { get; }

        public IDriveInfoFactory DriveInfo { get; }

        public IFileSystemWatcherFactory FileSystemWatcher { get; }
    }
}
