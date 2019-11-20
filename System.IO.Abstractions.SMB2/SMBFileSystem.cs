using System;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileSystem : IFileSystem
    {
        public SMBFileSystem(ISMBClientFactory ismbClientfactory, ISMBCredentialProvider credentialProvider)
        {
            File = new SMBFile(ismbClientfactory, credentialProvider, this);
            Directory = new SMBDirectory(ismbClientfactory, credentialProvider, this);
            DirectoryInfo = new SMBDirectoryInfoFactory(this, credentialProvider, ismbClientfactory);
            FileInfo = new SMBFileInfoFactory(this, credentialProvider, ismbClientfactory);
            FileStream = new SMBFileStreamFactory();
            Path = new PathWrapper(this);
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
