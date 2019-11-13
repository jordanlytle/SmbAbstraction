using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using SmbLibraryStd;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBDirectory : DirectoryWrapper, IDirectory
    {
        private ISMBClient _smbClient;
        private ISMBCredentialProvider _credentialProvider;

        public IPAddress ipAddress { get; set; }
        public SMBTransportType transport { get; set; }

        public SMBDirectory(ISMBClient smbclient, ISMBCredentialProvider credentialProvider) : base(new FileSystem())
        {
            _smbClient = smbclient;
            _credentialProvider = credentialProvider;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public override IDirectoryInfo CreateDirectory(string path)
        {
            if(!IsSMBPath(path))
            {
                return CreateDirectory(path);
            }

            throw new NotImplementedException();
        }

        public override void Delete(string path)
        {
            if(!IsSMBPath(path))
            {
                Delete(path);
            }

            throw new NotImplementedException();
        }

        public override void Delete(string path, bool recursive)
        {
            if(!IsSMBPath(path))
            {
                Delete(path, recursive);
            }

            throw new NotImplementedException();
        }

        public override IEnumerable<string> EnumerateDirectories(string path)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateDirectories(path);
            }

            return EnumerateDirectories(path, "*");
        }

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateDirectories(path, searchPattern);
            }

            return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            if (!IsSMBPath(path))
            {
                return EnumerateDirectories(path, searchPattern, searchOption);
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                throw new NotSupportedException();
            }

            Uri uri = new Uri(path);
            var hostEntry = Dns.GetHostEntry(uri.Host);
            ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (_smbClient.Connect(ipAddress, transport))
            {
                var credential = _credentialProvider.GetSMBCredential(path);
                status = _smbClient.Login(credential.GetDomain(), credential.GetUserName(), credential.GetPassword());

                var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
                var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1);

                ISMBFileStore fileStore = _smbClient.TreeConnect(shareName, out status);

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new IOException($"Unable to connect to smbShare. Status = {status}");
                }

                fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, searchPattern, FileInformationClass.FileDirectoryInformation);


                List<string> files = new List<string>();

                foreach (var file in queryDirectoryFileInformation)
                {
                    if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                    {
                        FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;

                        if (fileDirectoryInformation.FileName == "." || fileDirectoryInformation.FileName == "..")
                        {
                            continue;
                        }

                        if (fileDirectoryInformation.FileAttributes.HasFlag(SmbLibraryStd.FileAttributes.Directory))
                        {
                            files.Add(Path.Combine(path, fileDirectoryInformation.FileName));
                        }
                    }
                }
                fileStore.CloseFile(handle);

                return files;
            }

            return new List<string>();
        }

        public override IEnumerable<string> EnumerateFiles(string path)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateFiles(path);
            }

            return EnumerateFiles(path, "*");
        }

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateFiles(path, searchPattern);
            }

            return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateFiles(path, searchPattern, searchOption);
            }

            if(searchOption == SearchOption.AllDirectories)
            {
                throw new NotSupportedException();
            }

            Uri uri = new Uri(path);
            var hostEntry = Dns.GetHostEntry(uri.Host);
            ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (_smbClient.Connect(ipAddress, transport))
            {
                var credential = _credentialProvider.GetSMBCredential(path);
                status = _smbClient.Login(credential.GetDomain(), credential.GetUserName(), credential.GetPassword());

                var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
                var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1);

                ISMBFileStore fileStore = _smbClient.TreeConnect(shareName, out status);

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new IOException($"Unable to connect to smbShare. Status = {status}");
                }

                fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, searchPattern, FileInformationClass.FileDirectoryInformation);


                List<string> files = new List<string>();

                foreach (var file in queryDirectoryFileInformation)
                {
                    if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                    {
                        FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                        if (fileDirectoryInformation.FileName == "."
                            || fileDirectoryInformation.FileName == ".."
                            || fileDirectoryInformation.FileAttributes.HasFlag(SmbLibraryStd.FileAttributes.Directory))
                        {
                            continue;
                        }

                        files.Add(Path.Combine(path, fileDirectoryInformation.FileName));
                    }
                }
                fileStore.CloseFile(handle);

                return files;
            }

            return new List<string>();
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateFileSystemEntries(path);
            }

            return EnumerateFileSystemEntries(path, "*");
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateFileSystemEntries(path, searchPattern);
            }

            return EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            if(!IsSMBPath(path))
            {
                return EnumerateFileSystemEntries(path, searchPattern, searchOption);
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                throw new NotSupportedException();
            }

            Uri uri = new Uri(path);
            var hostEntry = Dns.GetHostEntry(uri.Host);
            ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (_smbClient.Connect(ipAddress, transport))
            {
                var credential = _credentialProvider.GetSMBCredential(path);
                status = _smbClient.Login(credential.GetDomain(), credential.GetUserName(), credential.GetPassword());

                var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
                var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1);

                ISMBFileStore fileStore = _smbClient.TreeConnect(shareName, out status);

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new IOException($"Unable to connect to smbShare. Status = {status}");
                }

                fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, searchPattern, FileInformationClass.FileDirectoryInformation);


                List<string> files = new List<string>();

                foreach (var file in queryDirectoryFileInformation)
                {
                    if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                    {
                        FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                        if (fileDirectoryInformation.FileName == "." || fileDirectoryInformation.FileName == "..")
                        {
                            continue;
                        }

                        files.Add(Path.Combine(path, fileDirectoryInformation.FileName));
                    }
                }
                fileStore.CloseFile(handle);

                return files;
            }

            return new List<string>();
        }

        public override bool Exists(string path)
        {
            if(!IsSMBPath(path))
            {
                return Exists(path);
            }

            throw new NotImplementedException();
        }

        public override DirectorySecurity GetAccessControl(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetAccessControl(path);
            }

            throw new NotImplementedException();
        }

        public override DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            if(!IsSMBPath(path))
            {
                return GetAccessControl(path, includeSections);
            }

            throw new NotImplementedException();
        }

        public override DateTime GetCreationTime(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetCreationTime(path);
            }

            throw new NotImplementedException();
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetCreationTimeUtc(path);
            }

            throw new NotImplementedException();
        }

        public override string GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        public override string[] GetDirectories(string path)
        {
            if (!IsSMBPath(path))
            {
                return GetDirectories(path);
            }

            return GetDirectories(path, "*");
        }

        public override string[] GetDirectories(string path, string searchPattern)
        {
            if(!IsSMBPath(path))
            {
                return GetDirectories(path, searchPattern);
            }

            return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            if(!IsSMBPath(path))
            {
                return GetDirectories(path, searchPattern, searchOption);
            }

            return EnumerateDirectories(path, searchPattern, searchOption).ToArray();
        }

        public override string GetDirectoryRoot(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetDirectoryRoot(path);
            }

            throw new NotImplementedException();
        }

        public override string[] GetFiles(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetFiles(path);
            }

            return GetFiles(path, "*");
        }

        public override string[] GetFiles(string path, string searchPattern)
        {
            if(!IsSMBPath(path))
            {
                return GetFiles(path, searchPattern);
            }

            return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if(!IsSMBPath(path))
            {
                return GetFiles(path, searchPattern, searchOption);
            }

            return EnumerateFiles(path, searchPattern, searchOption).ToArray();
        }

        public override string[] GetFileSystemEntries(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetFileSystemEntries(path);
            }

            return GetFileSystemEntries(path, "*");
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            if(!IsSMBPath(path))
            {
                return GetFileSystemEntries(path, searchPattern);
            }

            return EnumerateFileSystemEntries(path, searchPattern).ToArray();
        }

        public override DateTime GetLastAccessTime(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetLastAccessTime(path);
            }

            throw new NotImplementedException();
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetLastAccessTimeUtc(path);
            }

            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTime(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetLastWriteTime(path);
            }

            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetLastWriteTimeUtc(path);
            }

            throw new NotImplementedException();
        }

        public override IDirectoryInfo GetParent(string path)
        {
            if(!IsSMBPath(path))
            {
                return GetParent(path);
            }

            throw new NotImplementedException();
        }

        public override void Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        public override void SetAccessControl(string path, DirectorySecurity directorySecurity)
        {
            if(!IsSMBPath(path))
            {
                SetAccessControl(path, directorySecurity);
            }

            throw new NotImplementedException();
        }

        public override void SetCreationTime(string path, DateTime creationTime)
        {
            if(!IsSMBPath(path))
            {
                SetCreationTime(path, creationTime);
            }

            throw new NotImplementedException();
        }

        public override void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            if(!IsSMBPath(path))
            {
                SetCreationTimeUtc(path, creationTimeUtc);
            }

            throw new NotImplementedException();
        }

        public override void SetCurrentDirectory(string path)
        {
            if(!IsSMBPath(path))
            {
                SetCurrentDirectory(path);
            }

            throw new NotImplementedException();
        }

        public override void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            if(!IsSMBPath(path))
            {
                SetLastAccessTime(path, lastAccessTime);
            }

            throw new NotImplementedException();
        }

        public override void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            if(!IsSMBPath(path))
            {
                SetLastAccessTimeUtc(path, lastAccessTimeUtc);
            }

            throw new NotImplementedException();
        }

        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            if(!IsSMBPath(path))
            {
                SetLastWriteTime(path, lastWriteTime);
            }

            throw new NotImplementedException();
        }

        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            if(!IsSMBPath(path))
            {
                SetLastWriteTimeUtc(path, lastWriteTimeUtc);
            }

            throw new NotImplementedException();
        }

        private bool IsSMBPath(string path)
        {
            return new Uri(path).IsUnc || path.StartsWith("smb://");
        }
    }
}
