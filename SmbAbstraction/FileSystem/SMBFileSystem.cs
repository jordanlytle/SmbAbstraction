using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBFileSystem : IFileSystem
    {
        public SMBFileSystem(ISMBClientFactory ismbClientfactory, ISMBCredentialProvider credentialProvider, uint maxBufferSize = 65536, ILoggerFactory loggerFactory = null)
        {
            File = new SMBFile(ismbClientfactory, credentialProvider, this, maxBufferSize, loggerFactory);
            Directory = new SMBDirectory(ismbClientfactory, credentialProvider, this, maxBufferSize, loggerFactory);
            DirectoryInfo = new SMBDirectoryInfoFactory(this, credentialProvider, ismbClientfactory, maxBufferSize, loggerFactory);
            FileInfo = new SMBFileInfoFactory(this, credentialProvider, ismbClientfactory, maxBufferSize);
            FileStream = new SMBFileStreamFactory(this);
            Path = new SMBPath(this);
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
