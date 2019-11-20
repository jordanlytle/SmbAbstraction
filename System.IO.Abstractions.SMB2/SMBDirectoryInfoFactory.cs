using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using SmbLibraryStd;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBDirectoryInfoFactory : IDirectoryInfoFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider  _credentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private SMBDirectory _smbDirectory => _fileSystem.Directory as SMBDirectory;
        private SMBFile _smbFile => _fileSystem.File as SMBFile;
        private SMBFileInfoFactory _fileInfoFactory => _fileSystem.FileInfo as SMBFileInfoFactory;

        public SMBTransportType transport { get; set; }

        public SMBDirectoryInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider credentialProvider, ISMBClientFactory smbClientFactory)
        {
            _fileSystem = fileSystem;
            _credentialProvider = credentialProvider;
            _smbClientFactory = smbClientFactory;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public IDirectoryInfo FromDirectoryName(string directoryName)
        {
            return FromDirectoryName(directoryName, null);
        }

        internal IDirectoryInfo FromDirectoryName(string path, ISMBCredential credential)
        {
            if (path.IsValidSharePath())
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
            var newPath = path.RelativeSharePath();

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return null;
            }

            SMBDirectoryInfo directoryInfo = new SMBDirectoryInfo(path, _smbDirectory, _smbFile, this, _fileInfoFactory);

            status = fileStore.GetFileInformation(out FileInformation fileInfo, handle, FileInformationClass.FileBasicInformation); // If you call this with any other FileInformationClass value
                                                                                                                                    // it doesn't work for some reason
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return null;
            }

            FileBasicInformation fileDirectoryInformation = (FileBasicInformation)fileInfo;
            if (fileDirectoryInformation.CreationTime.Time.HasValue)
            {
                directoryInfo.CreationTime = fileDirectoryInformation.CreationTime.Time.Value;
                directoryInfo.CreationTimeUtc = directoryInfo.CreationTime.ToUniversalTime();
            }
            directoryInfo.FileSystem = _fileSystem;
            if (fileDirectoryInformation.LastAccessTime.Time.HasValue)
            {
                directoryInfo.LastAccessTime = fileDirectoryInformation.LastAccessTime.Time.Value;
                directoryInfo.LastAccessTimeUtc = directoryInfo.LastAccessTime.ToUniversalTime();
            }
            if (fileDirectoryInformation.LastWriteTime.Time.HasValue)
            {
                directoryInfo.LastWriteTime = fileDirectoryInformation.LastWriteTime.Time.Value;
                directoryInfo.LastWriteTimeUtc = directoryInfo.LastWriteTime.ToUniversalTime();
            }
            directoryInfo.Parent = _smbDirectory.GetParent(path, credential);
            var pathRoot = Path.GetPathRoot(path);
            if (pathRoot != string.Empty)
            {
                directoryInfo.Root = FromDirectoryName(pathRoot, credential);
            }
            return directoryInfo;
        }
    }
}
