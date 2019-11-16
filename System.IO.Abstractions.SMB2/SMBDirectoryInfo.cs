using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace System.IO.Abstractions.SMB
{
    public class SMBDirectoryInfo : IDirectoryInfo
    {
        private readonly SMBDirectory _smbDirctory;

        public SMBDirectoryInfo(string fileName, SMBDirectory smbDirectory)
        {
            _fullName = fileName;
            _smbDirctory = smbDirectory;
        }

        private readonly string _fullName;

        public IDirectoryInfo Parent { get; set; }

        public IDirectoryInfo Root { get; set; }

        public IFileSystem FileSystem { get; set; }

        public FileAttributes Attributes { get; set; }
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
            _smbDirctory.CreateDirectory(FullName);
        }

        public IDirectoryInfo CreateSubdirectory(string path)
        {
            return _smbDirctory.CreateDirectory(Path.Combine(FullName, path));
        }

        public void Delete(bool recursive)
        {
            _smbDirctory.Delete(FullName, recursive);
        }

        public void Delete()
        {
            _smbDirctory.Delete(FullName);
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
            var paths = _smbDirctory.EnumerateDirectories(FullName, searchPattern, searchOption);
            List<IDirectoryInfo> directoryInfos = new List<IDirectoryInfo>();
            foreach (var path in paths)
            {
                directoryInfos.Add(_smbDirctory.GetDirectoryInfo(path));
            }

            return directoryInfos;
        }

        public IEnumerable<IFileInfo> EnumerateFiles()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public DirectorySecurity GetAccessControl()
        {
            return _smbDirctory.GetAccessControl(FullName);
        }

        public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _smbDirctory.GetAccessControl(FullName, includeSections);
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
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            var info = _smbDirctory.GetDirectoryInfo(FullName);
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
            throw new NotImplementedException();
        }
    }
}
