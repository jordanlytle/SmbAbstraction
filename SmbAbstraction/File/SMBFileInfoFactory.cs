using System;
using System.IO;
using System.IO.Abstractions;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBFileInfoFactory : IFileInfoFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private readonly uint _maxBufferSize;
        private SMBDirectoryInfoFactory _dirInfoFactory => _fileSystem.DirectoryInfo as SMBDirectoryInfoFactory;

        public SMBTransportType transport { get; set; }

        public SMBFileInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider credentialProvider,
            ISMBClientFactory smbClientFactory, uint maxBufferSize)
        {
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
                throw new ArgumentException($"Unable to resolve \"{path.Hostname()}\"");
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new Exception($"Unable to find credential for path: {path}");
            }

            using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

            var shareName = path.ShareName();
            var relativePath = path.RelativeSharePath();

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status.HandleStatus();

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);

            status.HandleStatus();

            status = fileStore.GetFileInformation(out FileInformation fileBasicInfo, handle, FileInformationClass.FileBasicInformation); 
            status.HandleStatus();
            status = fileStore.GetFileInformation(out FileInformation fileStandardInfo, handle, FileInformationClass.FileStandardInformation);
            status.HandleStatus();

            fileStore.CloseFile(handle);

            return new SMBFileInfo(path, _fileSystem, (FileBasicInformation)fileBasicInfo, (FileStandardInformation)fileStandardInfo, credential);
        }

        internal void SaveFileInfo(SMBFileInfo fileInfo, ISMBCredential credential = null)
        {
            var path = fileInfo.FullName;

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new ArgumentException($"Unable to resolve \"{path.Hostname()}\"");
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new Exception($"Unable to find credential for path: {path}");
            }

            using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

            var shareName = path.ShareName();
            var relativePath = path.RelativeSharePath();

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_WRITE, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);

            status.HandleStatus();

            var smbFileInfo = fileInfo.ToSMBFileInformation(credential);
            status = fileStore.SetFileInformation(handle, smbFileInfo);

            status.HandleStatus();
        }
    }
}
