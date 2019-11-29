using SmbLibraryStd;
using SmbLibraryStd.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace System.IO.Abstractions.SMB
{
    public class SMBDriveInfoFactory : IDriveInfoFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _smbCredentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private readonly FileSystem _baseFileSystem;

        public SMBTransportType transport { get; set; }


        public SMBDriveInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider smbCredentialProvider, ISMBClientFactory smbClientFactory)
        {
            _fileSystem = fileSystem;
            _smbCredentialProvider = smbCredentialProvider;
            _smbClientFactory = smbClientFactory;
            _baseFileSystem = new FileSystem();
            transport = SMBTransportType.DirectTCPTransport;
        }

        public IDriveInfo FromDriveName(string driveName)
        {
            if (Uri.IsWellFormedUriString(driveName, UriKind.Absolute)
                && !driveName.IsSmbPath())
            {
                var driveInfo = new DriveInfo(driveName);
                return new DriveInfoWrapper(new FileSystem(), driveInfo);
            }

            return FromDriveName(driveName, null);
        }

        internal IDriveInfo FromDriveName(string shareName, ISMBCredential credential)
        {
            if (credential == null)
            {
                credential = _smbCredentialProvider.GetSMBCredentials().Where(c => c.Path.ShareName().Equals(shareName)).FirstOrDefault();

                if (credential == null)
                {
                    return null;
                }
            }

            var path = credential.Path;
            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new ArgumentException($"Unable to resolve \"{path.Hostname()}\"");
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential);

            var relativePath = path.RelativeSharePath();

            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

            status.HandleStatus();

            var smbFileSystemInformation = new SMBFileSystemInformation(fileStore, path);

            var smbDriveInfo = new SMBDriveInfo(path, _fileSystem, smbFileSystemInformation, credential);

            return smbDriveInfo;
        }

        public IDriveInfo[] GetDrives()
        {
            var drives = new List<IDriveInfo>();

            drives.AddRange(GetDrives(null));
            drives.AddRange(_baseFileSystem.DriveInfo.GetDrives());

            return drives.ToArray();
        }

        internal IDriveInfo[] GetDrives(ISMBCredential smbCredential)
        {
            var credentialsToCheck = new List<ISMBCredential>();
            credentialsToCheck = _smbCredentialProvider.GetSMBCredentials().ToList();

            if (smbCredential == null && credentialsToCheck.Count == 0)
            {
                return null;
            }

            List<IDriveInfo> driveInfos = new List<IDriveInfo>();

            NTStatus status = NTStatus.STATUS_SUCCESS;

            var shareHostNames = new List<string>();

            if (smbCredential != null)
            {
                credentialsToCheck.Add(smbCredential);
            }
            else
            {
                credentialsToCheck = _smbCredentialProvider.GetSMBCredentials().ToList();
            }

            shareHostNames = credentialsToCheck.Select(smbCredential => smbCredential.Path.Hostname()).Distinct().ToList();

            var shareHostShareNames = new Dictionary<string, IEnumerable<string>>();

            foreach (var shareHost in shareHostNames)
            {
                var credential = credentialsToCheck.Where(smbCredential => smbCredential.Path.Hostname().Equals(shareHost)).First();

                var path = credential.Path;
                if (!path.TryResolveHostnameFromPath(out var ipAddress))
                {
                    throw new ArgumentException($"Unable to resolve \"{path.Hostname()}\"");
                }

                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential);

                var shareNames = connection.SMBClient.ListShares(out status);
                var shareDirectoryInfoFactory = new SMBDirectoryInfoFactory(_fileSystem, _smbCredentialProvider, _smbClientFactory);

                foreach (var shareName in shareNames)
                {
                    var sharePath = path.BuildSharePath(shareName);
                    var relativeSharePath = sharePath.RelativeSharePath();

                    try
                    {
                        ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                        status.HandleStatus();

                        var smbFileSystemInformation = new SMBFileSystemInformation(fileStore, sharePath);

                        var smbDriveInfo = new SMBDriveInfo(sharePath, _fileSystem, smbFileSystemInformation, credential);

                        driveInfos.Add(smbDriveInfo);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }
            }

            return driveInfos.ToArray();
        }
    }
}
