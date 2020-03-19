using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.AccessControl;
using SMBLibrary;
using System.IO;

namespace SmbAbstraction
{
    public class SMBDirectoryInfo : DirectoryInfoWrapper, IDirectoryInfo
    {
        private readonly SMBDirectory _smbDirectory;
        private readonly SMBFile _smbFile;
        private readonly SMBDirectoryInfoFactory _directoryInfoFactory;
        private readonly SMBFileInfoFactory _fileInfoFactory;
        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly IFileSystem _fileSystem;

        public SMBDirectoryInfo(string fileName, SMBDirectory smbDirectory, SMBFile smbFile, SMBDirectoryInfoFactory directoryInfoFactory,
            SMBFileInfoFactory fileInfoFactory, IFileSystem fileSystem, ISMBCredentialProvider credentialProvider) : base(new FileSystem(), new DirectoryInfo(fileName))
        {
            _fullName = fileName;
            _smbDirectory = smbDirectory;
            _smbFile = smbFile;
            _directoryInfoFactory = directoryInfoFactory;
            _fileInfoFactory = fileInfoFactory;
            _credentialProvider = credentialProvider;
            _fileSystem = fileSystem;
        }

        internal SMBDirectoryInfo(DirectoryInfo directoryInfo, SMBDirectory smbDirectory, SMBFile smbFile, SMBDirectoryInfoFactory directoryInfoFactory, SMBFileInfoFactory fileInfoFactory, IFileSystem fileSystem, ISMBCredentialProvider credentialProvider)
            : this(directoryInfo.FullName, smbDirectory, smbFile, directoryInfoFactory, fileInfoFactory, fileSystem, credentialProvider)
        {
            _creationTime = directoryInfo.CreationTime;
            _creationTimeUtc = directoryInfo.CreationTimeUtc;
            _fileSystem = fileSystem;
            _lastAccessTime = directoryInfo.LastAccessTime;
            _lastAccessTimeUtc = directoryInfo.LastAccessTimeUtc;
            _lastWriteTime = directoryInfo.LastWriteTime;
            _lastWriteTimeUtc = directoryInfo.LastWriteTimeUtc;
            _parent = _directoryInfoFactory.FromDirectoryName(directoryInfo.Parent.FullName);
            _root = _directoryInfoFactory.FromDirectoryName(directoryInfo.Root.FullName);
            _exists = directoryInfo.Exists;
            _extension = directoryInfo.Extension;
            _name = directoryInfo.Name;
        }

        internal SMBDirectoryInfo(string fileName, SMBDirectory smbDirectory, SMBFile smbFile, SMBDirectoryInfoFactory directoryInfoFactory,
            SMBFileInfoFactory fileInfoFactory, FileInformation fileInfo, IFileSystem fileSystem, ISMBCredentialProvider credentialProvider, ISMBCredential credential)
            : this(fileName, smbDirectory, smbFile, directoryInfoFactory, fileInfoFactory, fileSystem, credentialProvider)
        {
            FileBasicInformation fileDirectoryInformation = (FileBasicInformation)fileInfo;
            if (fileDirectoryInformation.CreationTime.Time.HasValue)
            {
                _creationTime = fileDirectoryInformation.CreationTime.Time.Value;
                _creationTimeUtc = CreationTime.ToUniversalTime();
            }
            if (fileDirectoryInformation.LastAccessTime.Time.HasValue)
            {
                _lastAccessTime = fileDirectoryInformation.LastAccessTime.Time.Value;
                _lastAccessTimeUtc = LastAccessTime.ToUniversalTime();
            }
            if (fileDirectoryInformation.LastWriteTime.Time.HasValue)
            {
                _lastWriteTime = fileDirectoryInformation.LastWriteTime.Time.Value;
                _lastWriteTimeUtc = LastWriteTime.ToUniversalTime();
            }

            _parent = _smbDirectory.GetParent(fileName, credential);
            _fileSystem = fileSystem;
            var pathRoot = _fileSystem.Path.GetPathRoot(fileName);
            if (pathRoot != string.Empty && Parent != null)
            {
                _root = _directoryInfoFactory.FromDirectoryName(pathRoot, credential);
            }

            _exists = _fileSystem.Directory.Exists(FullName);
            _extension = string.Empty;
            _name = _fullName.GetLastPathSegment().RemoveAnySeperators();
        }

        private IDirectoryInfo _root;
        private IDirectoryInfo _parent;
        private System.IO.FileAttributes _attributes;
        private DateTime _creationTime;
        private DateTime _creationTimeUtc;
        private bool _exists;
        private string _extension;
        private string _fullName;
        private DateTime _lastAccessTime;
        private DateTime _lastAccessTimeUtc;
        private DateTime _lastWriteTime;
        private DateTime _lastWriteTimeUtc;
        private string _name;

        public override IDirectoryInfo Parent { get => _parent; }
        public override IDirectoryInfo Root { get => _root; }

        public override System.IO.FileAttributes Attributes 
        { 
            get => _attributes; 
            set => _attributes = value; 
        }

        public override DateTime CreationTime 
        { 
            get => _creationTime; 
            set => _creationTime = value; 
        }

        public override DateTime CreationTimeUtc 
        { 
            get => _creationTimeUtc;
            set => _creationTime = value;
        }

        public override bool Exists { get => _exists; }
        public override string Extension { get => _extension; }
        public override string FullName { get => _fullName; }

        public override DateTime LastAccessTime 
        { 
            get => _lastAccessTime;
            set => _lastAccessTime = value;
        }

        public override DateTime LastAccessTimeUtc 
        { 
            get => _lastAccessTimeUtc;
            set => _lastAccessTimeUtc = value;
        }

        public override DateTime LastWriteTime 
        { 
            get => _lastWriteTime;
            set => _lastWriteTime = value;
        }

        public override DateTime LastWriteTimeUtc 
        { 
            get => _lastWriteTimeUtc;
            set => _lastWriteTimeUtc = value;
        }

        public override string Name { get => _name; }

        public override void Create()
        {
            _smbDirectory.CreateDirectory(_fullName);
        }

        public override IDirectoryInfo CreateSubdirectory(string path)
        {
            return _smbDirectory.CreateDirectory(_fileSystem.Path.Combine(_fullName, path));
        }

        public override void Delete(bool recursive)
        {
            _smbDirectory.Delete(_fullName, recursive);
        }

        public override void Delete()
        {
            _smbDirectory.Delete(_fullName);
        }

        public override IEnumerable<IDirectoryInfo> EnumerateDirectories()
        {
            return EnumerateDirectories("*");
        }

        public override IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            var paths = _smbDirectory.EnumerateDirectories(_fullName, searchPattern, searchOption);

            var rootCredential = _credentialProvider.GetSMBCredential(_fullName);

            List<IDirectoryInfo> directoryInfos = new List<IDirectoryInfo>();
            foreach (var path in paths)
            {
                directoryInfos.Add(_directoryInfoFactory.FromDirectoryName(path, rootCredential));
            }

            return directoryInfos;
        }

        public override IEnumerable<IFileInfo> EnumerateFiles()
        {
            return EnumerateFiles("*");
        }

        public override IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            var paths = _smbDirectory.EnumerateFiles(FullName, searchPattern, searchOption);

            var rootCredential = _credentialProvider.GetSMBCredential(FullName);

            List<IFileInfo> fileInfos = new List<IFileInfo>();
            foreach (var path in paths)
            {
                fileInfos.Add(_fileInfoFactory.FromFileName(path, rootCredential));
            }

            return fileInfos;
        }

        public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
        {
            return EnumerateFileSystemInfos("*");
        }

        public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            var paths = _smbDirectory.EnumerateFileSystemEntries(_fullName, searchPattern, searchOption);

            var rootCredential = _credentialProvider.GetSMBCredential(_fullName);

            List<IFileSystemInfo> fileSystemInfos = new List<IFileSystemInfo>();
            foreach (var path in paths)
            {
                if (_smbFile.Exists(path))
                {
                    fileSystemInfos.Add(_fileInfoFactory.FromFileName(path, rootCredential));
                }
                else
                {
                    fileSystemInfos.Add(_directoryInfoFactory.FromDirectoryName(path, rootCredential));
                }
            }

            return fileSystemInfos;
        }

        public override DirectorySecurity GetAccessControl()
        {
            return _smbDirectory.GetAccessControl(_fullName);
        }

        public override DirectorySecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _smbDirectory.GetAccessControl(_fullName, includeSections);
        }

        public override IDirectoryInfo[] GetDirectories()
        {
            return EnumerateDirectories().ToArray();
        }

        public override IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern).ToArray();
        }

        public override IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(searchPattern, searchOption).ToArray();
        }

        public override IFileInfo[] GetFiles()
        {
            return EnumerateFiles().ToArray();
        }

        public override IFileInfo[] GetFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern).ToArray();
        }

        public override IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(searchPattern, searchOption).ToArray();
        }

        public override IFileSystemInfo[] GetFileSystemInfos()
        {
            return EnumerateFileSystemInfos().ToArray();
        }

        public override IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return EnumerateFileSystemInfos(searchPattern).ToArray();
        }

        public override IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
        }

        public override void MoveTo(string destDirName)
        {
            _smbDirectory.Move(_fullName, destDirName);
        }

        public override void Refresh()
        {
            var info = _directoryInfoFactory.FromDirectoryName(_fullName);
            _parent = info.Parent;
            _root = info.Root;
            _attributes = info.Attributes;
            _creationTime = info.CreationTime;
            _creationTimeUtc = info.CreationTimeUtc;
            _lastAccessTime = info.LastAccessTime;
            _lastAccessTimeUtc = info.LastAccessTimeUtc;
            _lastWriteTime = info.LastWriteTime;
            _lastWriteTimeUtc = info.LastWriteTimeUtc;
        }

        public override void SetAccessControl(DirectorySecurity directorySecurity)
        {
            _smbDirectory.SetAccessControl(_fullName, directorySecurity);
        }

        internal FileInformation ToSMBFileInformation(ISMBCredential credential = null)
        {
            FileBasicInformation fileBasicInformation = new FileBasicInformation();

            fileBasicInformation.CreationTime.Time = CreationTime;
            fileBasicInformation.LastAccessTime.Time = LastAccessTime;
            fileBasicInformation.LastWriteTime.Time = LastWriteTime;

            fileBasicInformation.FileAttributes = (SMBLibrary.FileAttributes)Attributes;

            return fileBasicInformation;
        }

        public override void Create(DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }
    }
}
