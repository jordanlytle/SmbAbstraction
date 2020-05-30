using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using SmbAbstraction.Utilities;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBDirectoryInfoFactory : IDirectoryInfoFactory
    {
        private readonly ILogger<SMBDirectoryInfoFactory> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private uint _maxBufferSize;
        private SMBDirectory _smbDirectory => _fileSystem.Directory as SMBDirectory;
        private SMBFile _smbFile => _fileSystem.File as SMBFile;
        private SMBFileInfoFactory _fileInfoFactory => _fileSystem.FileInfo as SMBFileInfoFactory;

        public SMBTransportType transport { get; set; }

        public SMBDirectoryInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider credentialProvider,
            ISMBClientFactory smbClientFactory, uint maxBufferSize, ILoggerFactory loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<SMBDirectoryInfoFactory>();
            _fileSystem = fileSystem;
            _credentialProvider = credentialProvider;
            _smbClientFactory = smbClientFactory;
            _maxBufferSize = maxBufferSize;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public IDirectoryInfo FromDirectoryName(string directoryName)
        {
            if (!directoryName.IsSharePath())
            {
                var dirInfo = new DirectoryInfo(directoryName);
                return new SMBDirectoryInfo(dirInfo, _fileSystem, _credentialProvider);
            }

            return FromDirectoryName(directoryName, null);
        }

        internal IDirectoryInfo FromDirectoryName(string path, ISMBCredential credential)
        {
            if (!path.IsSharePath() || !path.IsValidSharePath())
            {
                return null;
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed FromDirectoryName for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed FromDirectoryName for {path}", new InvalidCredentialException($"Unable to find credential for path: {path}"));
            }

            ISMBFileStore fileStore = null;
            object handle = null;

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying FromDirectoryName {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);
                
                fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                status.HandleStatus();

                status = fileStore.CreateFile(out handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);

                status.HandleStatus();

                status = fileStore.GetFileInformation(out FileInformation fileInfo, handle, FileInformationClass.FileBasicInformation); // If you call this with any other FileInformationClass value
                                                                                                                                        // it doesn't work for some reason
                if (status != NTStatus.STATUS_SUCCESS)
                {
                    return null;
                }

                return new SMBDirectoryInfo(path, fileInfo, _fileSystem, _credentialProvider, credential);
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed FromDirectoryName for {path}", ex);
            }
            finally
            {
                FileStoreUtilities.CloseFile(fileStore, ref handle);
            }
        }

        internal void SaveDirectoryInfo(SMBDirectoryInfo dirInfo, ISMBCredential credential = null)
        {
            var path = dirInfo.FullName;

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to SaveDirectoryInfo for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to SaveDirectoryInfo for {path}", new InvalidCredentialException($"Unable to find credential for path: {path}"));
            }

            ISMBFileStore fileStore = null;
            object handle = null;

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to SaveDirectoryInfo {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

                fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                status.HandleStatus();

                status = fileStore.CreateFile(out handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_WRITE, 0, ShareAccess.Read,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);

                status.HandleStatus();

                var fileInfo = dirInfo.ToSMBFileInformation(credential);
                status = fileStore.SetFileInformation(handle, fileInfo);

                status.HandleStatus();
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed to SaveDirectoryInfo for {path}", ex);
            }
            finally
            {
                FileStoreUtilities.CloseFile(fileStore, ref handle);
            }
        }
    }
}
