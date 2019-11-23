using System;
using System.Net;
using SmbLibraryStd;
using System.Linq;
using SmbLibraryStd.Client;
using System.Runtime.Serialization;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileInfoFactory : IFileInfoFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private SMBDirectoryInfoFactory _dirInfoFactory => _fileSystem.DirectoryInfo as SMBDirectoryInfoFactory;

        public SMBTransportType transport { get; set; }

        public SMBFileInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider credentialProvider, ISMBClientFactory smbClientFactory)
        {
            _fileSystem = fileSystem;
            _credentialProvider = credentialProvider;
            _smbClientFactory = smbClientFactory;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public IFileInfo FromFileName(string fileName)
        {
            return FromFileName(fileName, null);
        }

        internal IFileInfo FromFileName(string path, ISMBCredential credential)
        {
            if (!path.IsSmbPath())
            {
                return null;
            }

            var hostEntry = Dns.GetHostEntry(path.HostName());
            var ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new Exception($"Unable to find credential for path: {path}");
            }

            using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential);

            var shareName = path.ShareName();
            var relativePath = path.RelativeSharePath();

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return null;
            }

            status = fileStore.GetFileInformation(out FileInformation fileInfo, handle, FileInformationClass.FileBasicInformation); // If you call this with any other FileInformationClass value
                                                                                                                                    // it doesn't work for some reason
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return null;
            }

            return new SMBFileInfo(path, _fileSystem, fileInfo, credential);
        }

        internal void SaveFileInfo(SMBFileInfo fileInfo, ISMBCredential credential = null)
        {
            var path = fileInfo.FullName;

            var hostEntry = Dns.GetHostEntry(path.HostName());
            var ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new Exception($"Unable to find credential for path: {path}");
            }

            using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential);

            var shareName = path.ShareName();
            var relativePath = path.RelativeSharePath();

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_WRITE, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                throw new IOException($"Unable to open file. NTSTatus: {status}");
            }

            var smbFileInfo = fileInfo.ToSMBFileInformation(credential);
            status = fileStore.SetFileInformation(handle, smbFileInfo);

            if (status != NTStatus.STATUS_SUCCESS)
            {
                throw new IOException($"Unable to set file info. NTSTatus: {status}");
            }
        }
    }
}
