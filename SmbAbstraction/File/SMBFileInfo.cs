using System;
using System.Security.AccessControl;
using System.IO.Abstractions;
using SMBLibrary;
using System.IO;

namespace SmbAbstraction
{
    public class SMBFileInfo : IFileInfo
    {
        private SMBFile _file => FileSystem.File as SMBFile;
        private SMBFileInfoFactory _fileInfoFactory => FileSystem.FileInfo as SMBFileInfoFactory;
        private SMBDirectoryInfoFactory _dirInfoFactory => FileSystem.DirectoryInfo as SMBDirectoryInfoFactory;

        public SMBFileInfo(string path, IFileSystem fileSystem)
        {
            FullName = path;
            FileSystem = fileSystem;
        }

        internal SMBFileInfo(string path, IFileSystem fileSystem, FileInformation fileInformation, ISMBCredential credential) : this(path, fileSystem)
        {
            FileBasicInformation fileBasicInformation = (FileBasicInformation)fileInformation;
            if (fileBasicInformation.CreationTime.Time.HasValue)
            {
                CreationTime = fileBasicInformation.CreationTime.Time.Value;
                CreationTimeUtc = CreationTime.ToUniversalTime();
            }
            if (fileBasicInformation.LastAccessTime.Time.HasValue)
            {
                LastAccessTime = fileBasicInformation.LastAccessTime.Time.Value;
                LastAccessTimeUtc = LastAccessTime.ToUniversalTime();
            }
            if (fileBasicInformation.LastWriteTime.Time.HasValue)
            {
                LastWriteTime = fileBasicInformation.LastWriteTime.Time.Value;
                LastWriteTimeUtc = LastWriteTime.ToUniversalTime();
            }

            Attributes = (System.IO.FileAttributes)fileBasicInformation.FileAttributes;

            var pathUri = new Uri(path);
            var parentUri = pathUri.AbsoluteUri.EndsWith('/') ? new Uri(pathUri, "..") : new Uri(pathUri, ".");

            Directory = _dirInfoFactory.FromDirectoryName(parentUri.AbsoluteUri, credential);
            DirectoryName = Directory?.Name;
            Exists = true;
            IsReadOnly = fileBasicInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.ReadOnly);
            Length = fileBasicInformation.Length;
        }

        public IDirectoryInfo Directory { get; set; }

        public string DirectoryName { get; internal set; }

        public bool IsReadOnly { get; set; }

        public long Length { get; internal set; }

        public IFileSystem FileSystem { get; }

        public System.IO.FileAttributes Attributes { get; set; }
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
            var streamWriter = _file.CreateText(FullName);
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
            var stream = _file.Open(FullName, mode, access);
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
            var stream = _file.OpenRead(FullName);
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

        internal FileInformation ToSMBFileInformation(ISMBCredential credential = null)
        {
            FileBasicInformation fileBasicInformation = new FileBasicInformation();

            fileBasicInformation.CreationTime.Time = CreationTime;
            fileBasicInformation.LastAccessTime.Time = LastAccessTime;
            fileBasicInformation.LastWriteTime.Time = LastWriteTime;

            fileBasicInformation.FileAttributes = (SMBLibrary.FileAttributes)Attributes;

            var pathUri = new Uri(FullName);
            var parentUri = pathUri.AbsoluteUri.EndsWith('/') ? new Uri(pathUri, "..") : new Uri(pathUri, ".");

            if (IsReadOnly)
            {
                fileBasicInformation.FileAttributes |= SMBLibrary.FileAttributes.ReadOnly;
            }
            else
            {
                fileBasicInformation.FileAttributes &= SMBLibrary.FileAttributes.ReadOnly;
            }

            return fileBasicInformation;
        }
    }
}
