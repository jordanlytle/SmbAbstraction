using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBFileInfoFactory : IFileInfoFactory
    {
        private readonly ILogger<SMBFileInfoFactory> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private readonly uint _maxBufferSize;
        private SMBDirectoryInfoFactory _dirInfoFactory => _fileSystem.DirectoryInfo as SMBDirectoryInfoFactory;

        public SMBTransportType transport { get; set; }

        public SMBFileInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider credentialProvider,
            ISMBClientFactory smbClientFactory, uint maxBufferSize, ILoggerFactory loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<SMBFileInfoFactory>();
            _fileSystem = fileSystem;
            _credentialProvider = credentialProvider;
            _smbClientFactory = smbClientFactory;
            _maxBufferSize = maxBufferSize;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public IFileInfo FromFileName(string fileName)
        {
            if (!fileName.IsSharePath())
            {
                var fileInfo = new FileInfo(fileName);
                return new SMBFileInfo(fileInfo, _fileSystem);
            }

            return FromFileName(fileName, null);
        }

        internal IFileInfo FromFileName(string path, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                return null;
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed FromFileName for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed FromFileName for {path}", new InvalidCredentialException($"Unable to find credential for path: {path}"));
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying FromFileName {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

                ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                status.HandleStatus();

                AccessMask accessMask = AccessMask.SYNCHRONIZE | AccessMask.GENERIC_READ;
                ShareAccess shareAccess = ShareAccess.Read;
                CreateDisposition disposition = CreateDisposition.FILE_OPEN;
                CreateOptions createOptions = CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT | CreateOptions.FILE_NON_DIRECTORY_FILE;

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, accessMask, 0, shareAccess,
                    disposition, createOptions, null);

                status.HandleStatus();

                status = fileStore.GetFileInformation(out FileInformation fileBasicInfo, handle, FileInformationClass.FileBasicInformation);
                status.HandleStatus();
                status = fileStore.GetFileInformation(out FileInformation fileStandardInfo, handle, FileInformationClass.FileStandardInformation);
                status.HandleStatus();

                fileStore.CloseFile(handle);

                return new SMBFileInfo(path, _fileSystem, (FileBasicInformation)fileBasicInfo, (FileStandardInformation)fileStandardInfo, credential);
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed FromFileName for {path}", ex);
            }
        }

        internal void SaveFileInfo(SMBFileInfo fileInfo, ISMBCredential credential = null)
        {
            var path = fileInfo.FullName;

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to SaveFileInfo for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to SaveFileInfo for {path}", new InvalidCredentialException($"Unable to find credential for path: {path}"));
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to SaveFileInfo {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

                ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                status.HandleStatus();

                AccessMask accessMask = AccessMask.SYNCHRONIZE | AccessMask.GENERIC_WRITE;
                ShareAccess shareAccess = ShareAccess.Read;
                CreateDisposition disposition = CreateDisposition.FILE_OPEN;
                CreateOptions createOptions = CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT | CreateOptions.FILE_NON_DIRECTORY_FILE;

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, accessMask, 0, shareAccess,
                    disposition, createOptions, null);

                status.HandleStatus();

                var smbFileInfo = fileInfo.ToSMBFileInformation(credential);
                status = fileStore.SetFileInformation(handle, smbFileInfo);

                status.HandleStatus();
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed to SaveFileInfo for {path}", ex);
            }
        }
    }
}
