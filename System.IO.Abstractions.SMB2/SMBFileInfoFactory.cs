using System;
using System.Net;
using SmbLibraryStd;
using System.Linq;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileInfoFactory : IFileInfoFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private SMBDirectoryInfoFactory _dirInfoFactory => _fileSystem.DirectoryInfo as SMBDirectoryInfoFactory;
        private SMBDirectory _smbDirectory => _fileSystem.Directory as SMBDirectory;
        private SMBFile _smbFile => _fileSystem.File as SMBFile;

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
            Uri uri = new Uri(path);

            if (uri.Segments.Length < 2)
            {
                return null;
            }

            var hostEntry = Dns.GetHostEntry(uri.Host);
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

            var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
            var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1).Replace('/', '\\');

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return null;
            }

            SMBFileInfo smbFileInfo = new SMBFileInfo(path, _fileSystem);

            status = fileStore.GetFileInformation(out FileInformation fileInfo, handle, FileInformationClass.FileBasicInformation); // If you call this with any other FileInformationClass value
                                                                                                                                    // it doesn't work for some reason
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return null;
            }

            FileBasicInformation fileBasicInformation = (FileBasicInformation)fileInfo;
            if (fileBasicInformation.CreationTime.Time.HasValue)
            {
                smbFileInfo.CreationTime = fileBasicInformation.CreationTime.Time.Value;
                smbFileInfo.CreationTimeUtc = smbFileInfo.CreationTime.ToUniversalTime();
            }
            if (fileBasicInformation.LastAccessTime.Time.HasValue)
            {
                smbFileInfo.LastAccessTime = fileBasicInformation.LastAccessTime.Time.Value;
                smbFileInfo.LastAccessTimeUtc = smbFileInfo.LastAccessTime.ToUniversalTime();
            }
            if (fileBasicInformation.LastWriteTime.Time.HasValue)
            {
                smbFileInfo.LastWriteTime = fileBasicInformation.LastWriteTime.Time.Value;
                smbFileInfo.LastWriteTimeUtc = smbFileInfo.LastWriteTime.ToUniversalTime();
            }

            smbFileInfo.Attributes = (System.IO.FileAttributes)fileBasicInformation.FileAttributes;
            SMBDirectory smbDirectory = _fileSystem.Directory as SMBDirectory;

            var pathUri = new Uri(path);
            var parentUri = pathUri.AbsoluteUri.EndsWith('/') ? new Uri(pathUri, "..") : new Uri(pathUri, ".");

            smbFileInfo.Directory = _dirInfoFactory.FromDirectoryName(parentUri.AbsoluteUri, credential);
            smbFileInfo.DirectoryName = smbFileInfo.Directory?.Name;
            smbFileInfo.Exists = true;
            smbFileInfo.IsReadOnly = fileBasicInformation.FileAttributes.HasFlag(SmbLibraryStd.FileAttributes.ReadOnly);
            smbFileInfo.Length = fileBasicInformation.Length;
            return smbFileInfo;
        }
    }
}
