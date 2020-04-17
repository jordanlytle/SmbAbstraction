using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.AccessControl;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBDirectory : DirectoryWrapper, IDirectory
    {
        private readonly ILogger<SMBDirectory> _logger;
        private readonly ISMBClientFactory _smbClientFactory;
        private readonly IFileSystem _fileSystem;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly uint _maxBufferSize;
        private SMBDirectoryInfoFactory _directoryInfoFactory => _fileSystem.DirectoryInfo as SMBDirectoryInfoFactory;

        public SMBTransportType transport { get; set; }

        public SMBDirectory(ISMBClientFactory smbclientFactory, ISMBCredentialProvider credentialProvider,
                    IFileSystem fileSystem, uint maxBufferSize, ILoggerFactory loggerFactory = null) : base(new FileSystem())
        {
            _logger = loggerFactory?.CreateLogger<SMBDirectory>();
            _smbClientFactory = smbclientFactory;
            _credentialProvider = credentialProvider;
            _fileSystem = fileSystem;
            _maxBufferSize = maxBufferSize;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public override IDirectoryInfo CreateDirectory(string path)
        {
            return CreateDirectory(path, null);
        }

        private IDirectoryInfo CreateDirectory(string path, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                return base.CreateDirectory(path);
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to CreateDirectory {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            AccessMask accessMask = AccessMask.MAXIMUM_ALLOWED;
            ShareAccess shareAccess = ShareAccess.Read;
            CreateDisposition disposition = CreateDisposition.FILE_OPEN_IF;
            CreateOptions createOptions = CreateOptions.FILE_DIRECTORY_FILE;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to CreateDirectory {path}", new InvalidCredentialException($"Unable to find credential in SMBCredentialProvider for path: {path}"));
            }

            if(Exists(path))
            {
                return _directoryInfoFactory.FromDirectoryName(path);
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to CreateDirectory {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize);

                ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                status.HandleStatus();

                int attempts = 0;
                int allowedRetrys = 3;
                object handle;

                do
                {
                    attempts++;

                    _logger?.LogTrace($"Attempt {attempts} to CreateDirectory {path}");

                    status = fileStore.CreateFile(out handle, out FileStatus fileStatus, relativePath, accessMask, 0, shareAccess,
                    disposition, createOptions, null);

                    if (status == NTStatus.STATUS_OBJECT_PATH_NOT_FOUND)
                    {
                        CreateDirectory(path.GetParentPath(), credential);
                        status = fileStore.CreateFile(out handle, out fileStatus, relativePath, accessMask, 0, shareAccess,
                        disposition, createOptions, null);
                    }
                }
                while (status == NTStatus.STATUS_PENDING && attempts < allowedRetrys);

                status.HandleStatus();

                fileStore.CloseFile(handle);

                return _directoryInfoFactory.FromDirectoryName(path, credential);
            }
            catch(Exception ex)
            {
                throw new SMBException($"Failed to CreateDirectory {path}", ex);
            }
        }

        public override void Delete(string path)
        {
            Delete(path, null);
        }

        internal void Delete(string path, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                base.Delete(path);
                return;
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to Delete {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            if (!Exists(path))
            {
                return;
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to Delete {path}", new InvalidCredentialException($"Unable to find credential in SMBCredentialProvider for path: {path}"));
            }

            if (EnumerateFileSystemEntries(path).Count() > 0)
            {
                throw new SMBException($"Failed to Delete {path}", new IOException("Cannot delete directory. Directory is not empty."));
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to Delete {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using (var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize))
                {
                    ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                    status.HandleStatus();

                    int attempts = 0;
                    int allowedRetrys = 3;
                    object handle;

                    do
                    {
                        attempts++;

                        _logger?.LogTrace($"Attempt {attempts} to Delete {path}");

                        status = fileStore.CreateFile(out handle, out FileStatus fileStatus, relativePath, AccessMask.DELETE, 0, ShareAccess.Delete,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DELETE_ON_CLOSE, null);
                    }
                    while (status == NTStatus.STATUS_PENDING && attempts < allowedRetrys);

                    status.HandleStatus();

                    // This is the correct delete command, but it doesn't work for some reason. You have to use FILE_DELETE_ON_CLOSE
                    // fileStore.SetFileInformation(handle, new FileDispositionInformation());

                    fileStore.CloseFile(handle);
                }
            }
            catch(Exception ex)
            {
                throw new SMBException($"Failed to Delete {path}", ex);
            }
        }

        public override void Delete(string path, bool recursive)
        {
            Delete(path, recursive, null);
        }

        public void Delete(string path, bool recursive, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                base.Delete(path, recursive);
                return;
            }

            if (recursive)
            {
                if (!path.TryResolveHostnameFromPath(out var ipAddress))
                {
                    throw new SMBException($"Failed to Delete {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
                }

                NTStatus status = NTStatus.STATUS_SUCCESS;

                if (credential == null)
                {
                    credential = _credentialProvider.GetSMBCredential(path);
                }

                if (credential == null)
                {
                    throw new SMBException($"Failed to Delete {path}", new InvalidCredentialException("Unable to find credential in SMBCredentialProvider for path: {path}"));
                }

                try
                {
                    var shareName = path.ShareName();
                    var relativePath = path.RelativeSharePath();

                    _logger?.LogTrace($"Trying to Delete {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                    using (var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize))
                    {
                        ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                        status.HandleStatus();

                        int attempts = 0;
                        int allowedRetrys = 3;
                        object handle;

                        do
                        {
                            attempts++;

                            _logger?.LogTrace($"Attempt {attempts} to Delete {path}");

                            status = fileStore.CreateFile(out handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Delete,
                                CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                        }
                        while (status == NTStatus.STATUS_PENDING && attempts < allowedRetrys);

                        status.HandleStatus();

                        fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, "*", FileInformationClass.FileDirectoryInformation);

                        foreach (var file in queryDirectoryFileInformation)
                        {
                            if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                            {
                                FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                                if (fileDirectoryInformation.FileName == "."
                                    || fileDirectoryInformation.FileName == ".."
                                    || fileDirectoryInformation.FileName == ".DS_Store")
                                {
                                    continue;
                                }
                                else if (fileDirectoryInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory))
                                {
                                    Delete(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName), recursive, credential);
                                }

                                _fileSystem.File.Delete(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName));
                            }
                        }
                        fileStore.CloseFile(handle);

                        Delete(path, credential);
                    }
                }
                catch(Exception ex)
                {
                    throw new SMBException($"Failed to Delete {path}", ex);
                }
            }
            else
            {
                Delete(path);
            }
        }

        public override IEnumerable<string> EnumerateDirectories(string path)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateDirectories(path);
            }

            return EnumerateDirectories(path, "*");
        }

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateDirectories(path, searchPattern);
            }

            return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(path, searchPattern, searchOption, null);
        }

        private IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateDirectories(path, searchPattern, searchOption);
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to EnumerateDirectories for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to EnumerateDirectories for {path}", new InvalidCredentialException($"Unable to find credential in SMBCredentialProvider for path: {path}"));
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to EnumerateDirectories {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using (var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize))
                {

                    ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                    status.HandleStatus();

                    status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);

                    status.HandleStatus();

                    fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, searchPattern, FileInformationClass.FileDirectoryInformation);

                    _logger?.LogTrace($"Found {queryDirectoryFileInformation.Count} FileDirectoryInformation for {path}");

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

                            if (fileDirectoryInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory))
                            {
                                files.Add(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName));
                                if (searchOption == SearchOption.AllDirectories)
                                {
                                    files.AddRange(EnumerateDirectories(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName), searchPattern, searchOption, credential));
                                }
                            }
                        }
                    }
                    fileStore.CloseFile(handle);

                    return files;
                }
            }
            catch(Exception ex)
            {
                throw new SMBException($"Failed to EnumerateDirectories for {path}", ex);
            }
        }

        public override IEnumerable<string> EnumerateFiles(string path)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateFiles(path);
            }

            return EnumerateFiles(path, "*");
        }

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateFiles(path, searchPattern);
            }

            return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(path, searchPattern, searchOption, null);
        }

        private IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateFiles(path, searchPattern, searchOption);
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to EnumerateFiles for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to EnumerateFiles for {path}", new InvalidCredentialException($"Unable to find credential in SMBCredentialProvider for path: {path}"));
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to EnumerateFiles for {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using (var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize))
                {
                    ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                    status.HandleStatus();

                    status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);

                    status.HandleStatus();

                    fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, searchPattern, FileInformationClass.FileDirectoryInformation);

                    _logger?.LogTrace($"Found {queryDirectoryFileInformation.Count} FileDirectoryInformation for {path}");

                    List<string> files = new List<string>();

                    foreach (var file in queryDirectoryFileInformation)
                    {
                        if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                        {
                            FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                            if (fileDirectoryInformation.FileName == "."
                                || fileDirectoryInformation.FileName == ".."
                                || fileDirectoryInformation.FileName == ".DS_Store")
                            {
                                continue;
                            }

                            if (fileDirectoryInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory))
                            {
                                if (searchOption == SearchOption.AllDirectories)
                                {
                                    files.AddRange(EnumerateFiles(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName), searchPattern, searchOption, credential));
                                }
                            }
                            else
                            {
                                files.Add(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName.RemoveAnySeperators()));
                            }
                        }
                    }
                    fileStore.CloseFile(handle);

                    return files;
                }
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed to EnumerateFiles {path}", ex);
            }
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateFileSystemEntries(path);
            }

            return EnumerateFileSystemEntries(path, "*");
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateFileSystemEntries(path, searchPattern);
            }

            return EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
        }


        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateFileSystemEntries(path, searchPattern, searchOption, null);
        }

        private IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                return base.EnumerateFileSystemEntries(path, searchPattern, searchOption);
            }

            if (!path.TryResolveHostnameFromPath(out var ipAddress))
            {
                throw new SMBException($"Failed to EnumerateFileSystemEntries for {path}", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
            }

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (credential == null)
            {
                credential = _credentialProvider.GetSMBCredential(path);
            }

            if (credential == null)
            {
                throw new SMBException($"Failed to EnumerateFileSystemEntries for {path}", new InvalidCredentialException($"Unable to find credential in SMBCredentialProvider for path: {path}"));
            }

            try
            {
                var shareName = path.ShareName();
                var relativePath = path.RelativeSharePath();

                _logger?.LogTrace($"Trying to EnumerateFileSystemEntries {{RelativePath: {relativePath}}} for {{ShareName: {shareName}}}");

                using (var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize))
                {
                    ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                    status.HandleStatus();

                    status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);

                    status.HandleStatus();

                    fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, searchPattern, FileInformationClass.FileDirectoryInformation);

                    _logger?.LogTrace($"Found {queryDirectoryFileInformation.Count} FileDirectoryInformation for {path}");

                    List<string> files = new List<string>();

                    foreach (var file in queryDirectoryFileInformation)
                    {
                        if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                        {
                            FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                            if (fileDirectoryInformation.FileName == "." || fileDirectoryInformation.FileName == ".." || fileDirectoryInformation.FileName == ".DS_Store")
                            {
                                continue;
                            }


                            if (fileDirectoryInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.Directory))
                            {
                                if (searchOption == SearchOption.AllDirectories)
                                {
                                    files.AddRange(EnumerateFileSystemEntries(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName), searchPattern, searchOption, credential));
                                }
                            }

                            files.Add(_fileSystem.Path.Combine(path, fileDirectoryInformation.FileName));
                        }
                    }
                    fileStore.CloseFile(handle);

                    return files;
                }
            }
            catch (Exception ex)
            {
                throw new SMBException($"Failed to EnumerateFileSystemEntries for {path}", ex);
            }
        }

        public override bool Exists(string path)
        {
            if (!path.IsSharePath())
            {
                return base.Exists(path);
            }

            try
            {
                if (!path.TryResolveHostnameFromPath(out var ipAddress))
                {
                    throw new SMBException($"Failed to determine if {path} exists", new ArgumentException($"Unable to resolve \"{path.Hostname()}\""));
                }

                NTStatus status = NTStatus.STATUS_SUCCESS;

                var credential = _credentialProvider.GetSMBCredential(path);

                using (var connection = SMBConnection.CreateSMBConnection(_smbClientFactory, ipAddress, transport, credential, _maxBufferSize))
                {
                    var shareName = path.ShareName();
                    var relativePath = path.RelativeSharePath();

                    _logger?.LogTrace($"Trying to determine if {{RelativePath: {relativePath}}} Exists for {{ShareName: {shareName}}}");

                    if (string.IsNullOrEmpty(relativePath))
                    {
                        return true;
                    }

                    var parentFullPath = path.GetParentPath();
                    var parentPath = parentFullPath.RelativeSharePath();
                    var directoryName = path.GetLastPathSegment().RemoveAnySeperators();

                    ISMBFileStore fileStore = connection.SMBClient.TreeConnect(shareName, out status);

                    status.HandleStatus();

                    status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, parentPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                        CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);

                    status.HandleStatus();

                    fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, string.IsNullOrEmpty(directoryName) ? "*" : directoryName, FileInformationClass.FileDirectoryInformation);

                    foreach (var file in queryDirectoryFileInformation)
                    {
                        if (file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                        {
                            FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                            if (fileDirectoryInformation.FileName == directoryName)
                            {
                                fileStore.CloseFile(handle);
                                return true;
                            }
                        }
                    }

                    fileStore.CloseFile(handle);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogTrace(ex, $"Failed to determine if {path} exists.");
                return false;
            }
        }

        public override DirectorySecurity GetAccessControl(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetAccessControl(path);
            }

            throw new NotSupportedException();
        }

        public override DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            if (!path.IsSharePath())
            {
                return base.GetAccessControl(path, includeSections);
            }

            throw new NotSupportedException();
        }

        public override DateTime GetCreationTime(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetCreationTime(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path).CreationTime;
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetCreationTimeUtc(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path).CreationTimeUtc;
        }

        public override string GetCurrentDirectory()
        {
            return base.GetCurrentDirectory();
        }

        public override string[] GetDirectories(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetDirectories(path);
            }

            return GetDirectories(path, "*");
        }

        public override string[] GetDirectories(string path, string searchPattern)
        {
            if (!path.IsSharePath())
            {
                return base.GetDirectories(path, searchPattern);
            }

            return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            if (!path.IsSharePath())
            {
                return base.GetDirectories(path, searchPattern, searchOption);
            }

            return EnumerateDirectories(path, searchPattern, searchOption).ToArray();
        }

        public override string GetDirectoryRoot(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetDirectoryRoot(path);
            }

            return _fileSystem.Path.GetPathRoot(path);
        }

        public override string[] GetFiles(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetFiles(path);
            }

            return GetFiles(path, "*");
        }

        public override string[] GetFiles(string path, string searchPattern)
        {
            if (!path.IsSharePath())
            {
                return base.GetFiles(path, searchPattern);
            }

            return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if (!path.IsSharePath())
            {
                return base.GetFiles(path, searchPattern, searchOption);
            }

            return EnumerateFiles(path, searchPattern, searchOption).ToArray();
        }

        public override string[] GetFileSystemEntries(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetFileSystemEntries(path);
            }

            return GetFileSystemEntries(path, "*");
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            if (!path.IsSharePath())
            {
                return base.GetFileSystemEntries(path, searchPattern);
            }

            return EnumerateFileSystemEntries(path, searchPattern).ToArray();
        }

        public override DateTime GetLastAccessTime(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetLastAccessTime(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path).LastAccessTime;
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetLastAccessTimeUtc(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path).LastAccessTimeUtc;
        }

        public override DateTime GetLastWriteTime(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetLastWriteTime(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path).LastWriteTime;
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetLastWriteTimeUtc(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path).LastWriteTimeUtc;
        }

        public override IDirectoryInfo GetParent(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetParent(path);
            }

            return GetParent(path, null);
        }

        internal IDirectoryInfo GetParent(string path, ISMBCredential credential)
        {
            if (!path.IsSharePath())
            {
                return base.GetParent(path);
            }

            return _directoryInfoFactory.FromDirectoryName(path.GetParentPath(), credential);
        }

        public override void Move(string sourceDirName, string destDirName)
        {
            Move(sourceDirName, destDirName, null, null);
        }

        private void Move(string sourceDirName, string destDirName, ISMBCredential sourceCredential, ISMBCredential destinationCredential)
        {
            if (sourceCredential == null)
            {
                sourceCredential = _credentialProvider.GetSMBCredential(sourceDirName);
            }

            if (destinationCredential == null)
            {
                destinationCredential = _credentialProvider.GetSMBCredential(destDirName);
            }

            CreateDirectory(destDirName, destinationCredential);

            var dirs = EnumerateDirectories(sourceDirName, "*", SearchOption.TopDirectoryOnly, sourceCredential);

            foreach (var dir in dirs)
            {
                var destDirPath = _fileSystem.Path.Combine(destDirName, new Uri(dir).Segments.Last());
                Move(dir, destDirPath, sourceCredential, destinationCredential);
            }

            var files = EnumerateFiles(sourceDirName, "*", SearchOption.TopDirectoryOnly, sourceCredential);

            foreach (var file in files)
            {
                var destFilePath = _fileSystem.Path.Combine(destDirName, new Uri(file).Segments.Last());
                SMBFile smbFile = _fileSystem.File as SMBFile;
                smbFile.Move(file, destFilePath, sourceCredential, destinationCredential);
            }
        }

        public override void SetAccessControl(string path, DirectorySecurity directorySecurity)
        {
            if (!path.IsSharePath())
            {
                base.SetAccessControl(path, directorySecurity);
                return;
            }

            throw new NotSupportedException();
        }

        public override void SetCreationTime(string path, DateTime creationTime)
        {
            if (!path.IsSharePath())
            {
                base.SetCreationTime(path, creationTime);
                return;
            }

            var dirInfo = _directoryInfoFactory.FromDirectoryName(path);
            dirInfo.CreationTime = creationTime.ToUniversalTime();
            _directoryInfoFactory.SaveDirectoryInfo((SMBDirectoryInfo)dirInfo);
        }

        public override void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            if (!path.IsSharePath())
            {
                base.SetCreationTimeUtc(path, creationTimeUtc);
                return;
            }

            var dirInfo = _directoryInfoFactory.FromDirectoryName(path);
            dirInfo.CreationTime = creationTimeUtc;
            _directoryInfoFactory.SaveDirectoryInfo((SMBDirectoryInfo)dirInfo);
        }

        public override void SetCurrentDirectory(string path)
        {
            if (!path.IsSharePath())
            {
                base.SetCurrentDirectory(path);
                return;
            }

            throw new NotImplementedException();
        }

        public override void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            if (!path.IsSharePath())
            {
                base.SetLastAccessTime(path, lastAccessTime);
                return;
            }

            var dirInfo = _directoryInfoFactory.FromDirectoryName(path);
            dirInfo.LastAccessTime = lastAccessTime.ToUniversalTime();
            _directoryInfoFactory.SaveDirectoryInfo((SMBDirectoryInfo)dirInfo);
        }

        public override void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            if (!path.IsSharePath())
            {
                base.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
                return;
            }

            var dirInfo = _directoryInfoFactory.FromDirectoryName(path);
            dirInfo.LastAccessTime = lastAccessTimeUtc;
            _directoryInfoFactory.SaveDirectoryInfo((SMBDirectoryInfo)dirInfo);
        }

        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            if (!path.IsSharePath())
            {
                base.SetLastWriteTime(path, lastWriteTime);
                return;
            }

            var dirInfo = _directoryInfoFactory.FromDirectoryName(path);
            dirInfo.LastWriteTime = lastWriteTime.ToUniversalTime();
            _directoryInfoFactory.SaveDirectoryInfo((SMBDirectoryInfo)dirInfo);
        }

        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            if (!path.IsSharePath())
            {
                base.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
                return;
            }

            var dirInfo = _directoryInfoFactory.FromDirectoryName(path);
            dirInfo.LastWriteTime = lastWriteTimeUtc;
            _directoryInfoFactory.SaveDirectoryInfo((SMBDirectoryInfo)dirInfo);
        }
    }
}
