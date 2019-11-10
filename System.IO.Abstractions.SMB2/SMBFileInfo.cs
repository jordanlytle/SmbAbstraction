using System;
using System.Security.AccessControl;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileInfo : IFileInfo
    {
        public SMBFileInfo()
        {
        }

        public IDirectoryInfo Directory => throw new NotImplementedException();

        public string DirectoryName => throw new NotImplementedException();

        public bool IsReadOnly { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public long Length => throw new NotImplementedException();

        public IFileSystem FileSystem => throw new NotImplementedException();

        public FileAttributes Attributes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime CreationTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime CreationTimeUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Exists => throw new NotImplementedException();

        public string Extension => throw new NotImplementedException();

        public string FullName => throw new NotImplementedException();

        public DateTime LastAccessTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime LastAccessTimeUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime LastWriteTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime LastWriteTimeUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => throw new NotImplementedException();

        public StreamWriter AppendText()
        {
            throw new NotImplementedException();
        }

        public IFileInfo CopyTo(string destFileName)
        {
            throw new NotImplementedException();
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public Stream Create()
        {
            throw new NotImplementedException();
        }

        public StreamWriter CreateText()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public FileSecurity GetAccessControl()
        {
            throw new NotImplementedException();
        }

        public FileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public void MoveTo(string destFileName)
        {
            throw new NotImplementedException();
        }

        public Stream Open(FileMode mode)
        {
            throw new NotImplementedException();
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead()
        {
            throw new NotImplementedException();
        }

        public StreamReader OpenText()
        {
            throw new NotImplementedException();
        }

        public Stream OpenWrite()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SetAccessControl(FileSecurity fileSecurity)
        {
            throw new NotImplementedException();
        }
    }
}
