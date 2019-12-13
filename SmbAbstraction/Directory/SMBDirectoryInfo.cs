using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.AccessControl;
using SMBLibrary;
using System.IO;

namespace SmbAbstraction
{
    public class SMBDirectoryInfo : IDirectoryInfo
    {
        private readonly SMBDirectory _smbDirectory;
        private readonly SMBFile _smbFile;
        private readonly SMBDirectoryInfoFactory _directoryInfoFactory;
        private readonly SMBFileInfoFactory _fileInfoFactory;

        public SMBDirectoryInfo(string fileName, SMBDirectory smbDirectory, SMBFile smbFile, SMBDirectoryInfoFactory directoryInfoFactory,
            SMBFileInfoFactory fileInfoFactory)
        {
            _fullName = fileName;
            _smbDirectory = smbDirectory;
            _smbFile = smbFile;
            _directoryInfoFactory = directoryInfoFactory;
            _fileInfoFactory = fileInfoFactory;
        }

        internal SMBDirectoryInfo(string fileName, SMBDirectory smbDirectory, SMBFile smbFile, SMBDirectoryInfoFactory directoryInfoFactory,
            SMBFileInfoFactory fileInfoFactory, FileInformation fileInfo, ISMBCredential credential)
            : this(fileName, smbDirectory, smbFile, directoryInfoFactory, fileInfoFactory)
        {
            FileBasicInformation fileDirectoryInformation = (FileBasicInformation)fileInfo;
            if (fileDirectoryInformation.CreationTime.Time.HasValue)
            {
                CreationTime = fileDirectoryInformation.CreationTime.Time.Value;
                CreationTimeUtc = CreationTime.ToUniversalTime();
            }
            FileSystem = FileSystem;
            if (fileDirectoryInformation.LastAccessTime.Time.HasValue)
            {
                LastAccessTime = fileDirectoryInformation.LastAccessTime.Time.Value;
                LastAccessTimeUtc = LastAccessTime.ToUniversalTime();
            }
            if (fileDirectoryInformation.LastWriteTime.Time.HasValue)
            {
                LastWriteTime = fileDirectoryInformation.LastWriteTime.Time.Value;
                LastWriteTimeUtc = LastWriteTime.ToUniversalTime();
            }
            Parent = _smbDirectory.GetParent(fileName, credential);
            var pathRoot = Path.GetPathRoot(fileName);
            if (pathRoot != string.Empty && Parent != null)
            {
                Root = _directoryInfoFactory.FromDirectoryName(pathRoot, credential);
            }
        }

        private readonly string _fullName;

        public IDirectoryInfo Parent { get; set; }

        public IDirectoryInfo Root { get; set; }

        public IFileSystem FileSystem { get; set; }

        public System.IO.FileAttributes Attributes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime CreationTimeUtc { get; set; }

        public bool Exists => FileSystem.Directory.Exists(FullName);

        public string Extension => FileSystem.Path.GetExtension(_fullName);

        public string FullName => _fullName;

        public DateTime LastAccessTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public string Name => Path.GetFileName(_fullName);

        public void Create()
        {
            _smbDirectory.CreateDirectory(FullName);
        }

        public IDirectoryInfo CreateSubdirectory(string path)
        {
            return _smbDirectory.CreateDirectory(Path.Combine(FullName, path));
        }

        public void Delete(bool recursive)
        {
            _smbDirectory.Delete(FullName, recursive);
        }

        public void Delete()
        {
            _smbDirectory.Delete(FullName);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories()
        {
            return EnumerateDirectories("*");
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            var paths = _smbDirectory.EnumerateDirectories(FullName, searchPattern, searchOption);
            List<IDirectoryInfo> directoryInfos = new List<IDirectoryInfo>();
            foreach (var path in paths)
            {
                directoryInfos.Add(_directoryInfoFactory.FromDirectoryName(path));
            }

            return directoryInfos;
        }

        public IEnumerable<IFileInfo> EnumerateFiles()
        {
            return EnumerateFiles("*");
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            var paths = _smbDirectory.EnumerateFiles(FullName, searchPattern, searchOption);
            List<IFileInfo> fileInfos = new List<IFileInfo>();
            foreach (var path in paths)
            {
                fileInfos.Add(_fileInfoFactory.FromFileName(path));
            }

            return fileInfos;
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
        {
            return EnumerateFileSystemInfos("*");
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            var paths = _smbDirectory.EnumerateFileSystemEntries(FullName, searchPattern, searchOption);
            List<IFileSystemInfo> fileSystemInfos = new List<IFileSystemInfo>();
            foreach (var path in paths)
            {
                if (_smbFile.Exists(path))
                {
                    fileSystemInfos.Add(_fileInfoFactory.FromFileName(path));
                }
                else
                {
                    fileSystemInfos.Add(_directoryInfoFactory.FromDirectoryName(path));
                }
            }

            return fileSystemInfos;
        }

        public DirectorySecurity GetAccessControl()
        {
            return _smbDirectory.GetAccessControl(FullName);
        }

        public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _smbDirectory.GetAccessControl(FullName, includeSections);
        }

        public IDirectoryInfo[] GetDirectories()
        {
            return EnumerateDirectories().ToArray();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern).ToArray();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(searchPattern, searchOption).ToArray();
        }

        public IFileInfo[] GetFiles()
        {
            return EnumerateFiles().ToArray();
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern).ToArray();
        }

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(searchPattern, searchOption).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos()
        {
            return EnumerateFileSystemInfos().ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return EnumerateFileSystemInfos(searchPattern).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
        }

        public void MoveTo(string destDirName)
        {
            _smbDirectory.Move(_fullName, destDirName);
        }

        public void Refresh()
        {
            var info = _directoryInfoFactory.FromDirectoryName(FullName);
            Parent = info.Parent;
            Root = info.Root;
            FileSystem = info.FileSystem;
            Attributes = info.Attributes;
            CreationTime = info.CreationTime;
            CreationTimeUtc = info.CreationTimeUtc;
            LastAccessTime = info.LastAccessTime;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            LastWriteTime = info.LastWriteTime;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
        }

        public void SetAccessControl(DirectorySecurity directorySecurity)
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
    }
}
