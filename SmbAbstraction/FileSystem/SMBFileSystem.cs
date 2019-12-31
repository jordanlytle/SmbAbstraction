using System;
using System.IO.Abstractions;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBFileSystem : IFileSystem
    {
        public SMBFileSystem(ISMBClientFactory ismbClientfactory, ISMBCredentialProvider credentialProvider, uint maxBufferSize = 65536)
        {
            File = new SMBFile(ismbClientfactory, credentialProvider, this, maxBufferSize);
            Directory = new SMBDirectory(ismbClientfactory, credentialProvider, this, maxBufferSize);
            DirectoryInfo = new SMBDirectoryInfoFactory(this, credentialProvider, ismbClientfactory, maxBufferSize);
            FileInfo = new SMBFileInfoFactory(this, credentialProvider, ismbClientfactory, maxBufferSize);
            FileStream = new SMBFileStreamFactory(this);
            Path = new PathWrapper(this);
            DriveInfo = new SMBDriveInfoFactory(this, credentialProvider, ismbClientfactory, maxBufferSize);
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
