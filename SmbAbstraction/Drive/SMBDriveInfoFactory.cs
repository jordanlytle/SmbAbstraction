using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBDriveInfoFactory : IDriveInfoFactory
    {
        private readonly ILogger<SMBDriveInfoFactory> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _smbCredentialProvider;
        private readonly ISMBClientFactory _smbClientFactory;
        private readonly FileSystem _baseFileSystem;
        private readonly uint _maxBufferSize;

        public SMBTransportType transport { get; set; }


        public SMBDriveInfoFactory(IFileSystem fileSystem, ISMBCredentialProvider smbCredentialProvider,
            ISMBClientFactory smbClientFactory, uint maxBufferSize, ILoggerFactory loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<SMBDriveInfoFactory>();
            _fileSystem = fileSystem;
            _smbCredentialProvider = smbCredentialProvider;
            _smbClientFactory = smbClientFactory;
            _baseFileSystem = new FileSystem();
            _maxBufferSize = maxBufferSize;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public IDriveInfo FromDriveName(string driveName)
        {
            if(string.IsNullOrEmpty(driveName))
            {
                throw new SMBException($"Failed FromDriveName", new ArgumentException("Drive name cannot be null or empty.", nameof(driveName)));
            }

            if (driveName.IsSharePath() || PossibleShareName(driveName))
            {
                return FromDriveName(driveName, null);
            }

            var driveInfo = new DriveInfo(driveName);
            return new DriveInfoWrapper(new FileSystem(), driveInfo);
        }

        internal IDriveInfo FromDriveName(string shareName, ISMBCredential credential)
        {
            if (credential == null)
            {
                if(shareName.IsValidSharePath())
                {
                    credential = _smbCredentialProvider.GetSMBCredentials().Where(c => c.Path.SharePath().Equals(shareName)).FirstOrDefault();
                    shareName = shareName.ShareName();
                }
                else
                {
                    credential = _smbCredentialProvider.GetSMBCredentials().Where(c => c.Path.ShareName().Equals(shareName)).FirstOrDefault();
                }

                if (credential == null)
                {
                    _logger?.LogTrace($"Unable to find credential in SMBCredentialProvider for path: {shareName}");
                    return null;
                }
            }

            var path = credential.Path.SharePath();
            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed FromDriveName for {shareName}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;
            try
            {
                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

                var relativePath = path.RelativeSharePath();

                ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                status.HandleStatus();

                var smbFileSystemInformation = new SMBFileSystemInformation(fileStore, path, status);

                var smbDriveInfo = new SMBDriveInfo(path, _fileSystem, smbFileSystemInformation, credential);

                return smbDriveInfo;
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed FromDriveName for {shareName}", ex);
            }
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

            List<IDriveInfo> driveInfos = new List<IDriveInfo>();

            if (smbCredential == null && credentialsToCheck.Count == 0)
            {
                _logger?.LogTrace($"No provided credentials and no credentials stored credentials in SMBCredentialProvider.");
                return driveInfos.ToArray();
            }

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
                try
                {
                    var path = credential.Path;
                    if (!path.TryResolveHostnameFromPath(out var ipAddress))
                    {
                        throw new SMBException($"Failed to connect to {path.Hostname()}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
                    }

                    using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

                    var shareNames = connection.SMBClient.ListShares(out status);
                    var shareDirectoryInfoFactory = new SMBDirectoryInfoFactory(_fileSystem, _smbCredentialProvider, _smbClientFactory, _maxBufferSize);

                    foreach (var shareName in shareNames)
                    {
                        var sharePath = path.BuildSharePath(shareName);
                        var relativeSharePath = sharePath.RelativeSharePath();

                        _logger?.LogTrace($"Trying to get drive info for {shareName}");

                        try
                        {
                            ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                            status.HandleStatus();

                            var smbFileSystemInformation = new SMBFileSystemInformation(fileStore, sharePath, status);

                            var smbDriveInfo = new SMBDriveInfo(sharePath, _fileSystem, smbFileSystemInformation, credential);

                            driveInfos.Add(smbDriveInfo);
                        }
                        catch (IOException ioEx)
                        {
                            _logger?.LogTrace(ioEx, $"Failed to get drive info for {shareName}");
                            throw new SMBException($"Failed to get drive info for {shareName}", new AggregateException($"Unable to connect to {shareName}", ioEx));
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogTrace(ex, $"Failed to get drive info for {shareName}");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogTrace(ex,$"Failed to GetDrives for {shareHost}.");
                    continue;
                }
            }

            return driveInfos.ToArray();
        }

        private bool IsDriveLetter(string driveName)
        {
            return ((driveName.Length == 1 || driveName.EndsWith(@":\")) && Char.IsLetter(driveName, 0));
        }

        private bool PossibleShareName(string input)
        {
            return DriveInfo.GetDrives().All(d => (d.Name != input) && !IsDriveLetter(input));
        }
    }
}
