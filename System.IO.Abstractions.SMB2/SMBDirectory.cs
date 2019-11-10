using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Security.AccessControl;

namespace System.IO.Abstractions.SMB
{
    public class SMBDirectory : IDirectory
    {
        public SMBDirectory()
        {
        }

        public IFileSystem FileSystem => throw new NotImplementedException();

        public IDirectoryInfo CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path)
        {
            throw new NotImplementedException();
        }

        public DirectorySecurity GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }

        public DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public DateTime GetCreationTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        public string[] GetDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetDirectories(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public string GetDirectoryRoot(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetFiles(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastAccessTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public IDirectoryInfo GetParent(string path)
        {
            throw new NotImplementedException();
        }

        public void Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        public void SetAccessControl(string path, DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }
    }
}
