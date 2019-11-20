using System;
using System.Security.AccessControl;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileInfo : IFileInfo
    {
        private SMBFile _file => FileSystem.File as SMBFile;
        private SMBFileInfoFactory _fileInfoFactory => FileSystem.FileInfo as SMBFileInfoFactory;

        public SMBFileInfo(string path, IFileSystem fileSystem)
        {
            FullName = path;
            FileSystem = fileSystem;
        }

        public IDirectoryInfo Directory { get; set; }

        public string DirectoryName { get; internal set; }

        public bool IsReadOnly { get; set; }

        public long Length { get; internal set; }

        public IFileSystem FileSystem { get; }

        public FileAttributes Attributes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime CreationTimeUtc { get; set; }

        public bool Exists { get; internal set; }

        public string Extension => Path.GetExtension(FullName);

        public string FullName { get; private set; }

        public DateTime LastAccessTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public string Name => Path.GetFileName(FullName);

        public StreamWriter AppendText()
        {
            return _file.AppendText(FullName);
        }

        public IFileInfo CopyTo(string destFileName)
        {
            _file.Copy(FullName, destFileName);
            return _fileInfoFactory.FromFileName(destFileName);
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            _file.Copy(FullName, destFileName, overwrite);
            return _fileInfoFactory.FromFileName(destFileName);
        }

        public Stream Create()
        {
            var stream = _file.Create(FullName);
            Exists = true;
            return stream;
        }

        public StreamWriter CreateText()
        {
            var streamWriter =  _file.CreateText(FullName);
            Exists = true;
            return streamWriter;
        }

        public void Delete()
        {
            _file.Delete(FullName);
            Exists = false;
        }

        public FileSecurity GetAccessControl()
        {
            return _file.GetAccessControl(FullName);
        }

        public FileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _file.GetAccessControl(FullName, includeSections);
        }

        public void MoveTo(string destFileName)
        {
            _file.Move(FullName, destFileName);
        }

        public Stream Open(FileMode mode)
        {
            var stream = _file.Open(FullName, mode);
            Exists = true;
            return stream;
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            var stream =  _file.Open(FullName, mode, access);
            Exists = true;
            return stream;
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            var stream = _file.Open(FullName, mode, access, share);
            Exists = true;
            return stream;
        }

        public Stream OpenRead()
        {
            var stream =  _file.OpenRead(FullName);
            Exists = true;
            return stream;
        }

        public StreamReader OpenText()
        {
            var streamReader = _file.OpenText(FullName);
            Exists = true;
            return streamReader;
        }

        public Stream OpenWrite()
        {
            var stream = _file.OpenWrite(FullName);
            Exists = true;
            return stream;
        }

        public void Refresh()
        {
            var fileInfo = _fileInfoFactory.FromFileName(FullName);

            Directory = fileInfo.Directory;
            DirectoryName = fileInfo.DirectoryName;
            IsReadOnly = fileInfo.IsReadOnly;
            Length = fileInfo.Length;
            Attributes = fileInfo.Attributes;
            CreationTime = fileInfo.CreationTime;
            CreationTimeUtc = fileInfo.CreationTimeUtc;
            Exists = fileInfo.Exists;
            FullName = fileInfo.FullName;
            LastAccessTime = fileInfo.LastAccessTime;
            LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            LastWriteTime = fileInfo.LastWriteTime;
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
        }

        public void SetAccessControl(FileSecurity fileSecurity)
        {
            _file.SetAccessControl(FullName, fileSecurity);
        }
    }
}
