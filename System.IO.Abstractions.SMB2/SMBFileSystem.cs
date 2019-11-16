using System;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileSystem : IFileSystem
    {
        public SMBFileSystem(ISMBClientFactory ismbClientfactory, ISMBCredentialProvider credentialProvider)
        {
            Directory = new SMBDirectory(ismbClientfactory, credentialProvider, this);
            File = new SMBFile(ismbClientfactory, credentialProvider);
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
