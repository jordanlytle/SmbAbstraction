using System;
using System.Security.AccessControl;
using System.IO.Abstractions;
using SMBLibrary;
using System.IO;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBFileInfo : FileInfoWrapper, IFileInfo
    {
        private SMBFile _file => _fileSystem.File as SMBFile;
        private SMBFileInfoFactory _fileInfoFactory => _fileSystem.FileInfo as SMBFileInfoFactory;
        private SMBDirectoryInfoFactory _dirInfoFactory => _fileSystem.DirectoryInfo as SMBDirectoryInfoFactory;
        private readonly IFileSystem _fileSystem;

        public SMBFileInfo(string path, 
                           IFileSystem fileSystem): base(new FileSystem(), new FileInfo(path))
        {
            _fullName = path;
            _fileSystem = fileSystem;
        }

        internal SMBFileInfo(FileInfo fileInfo, 
                             IFileSystem fileSystem) : this(fileInfo.FullName, fileSystem)
        {
            _creationTime = fileInfo.CreationTime;
            _creationTimeUtc = fileInfo.CreationTimeUtc;
            _lastAccessTime = fileInfo.LastAccessTime;
            _lastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            _lastWriteTime = fileInfo.LastWriteTime;
            _lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            _attributes = fileInfo.Attributes;
            _directory = _dirInfoFactory.FromDirectoryName(fileInfo.Directory.FullName);
            _directoryName = fileInfo.DirectoryName;
            _exists = fileInfo.Exists;
            _isReadOnly = fileInfo.IsReadOnly;
            _length = fileInfo.Length;
        }

        internal SMBFileInfo(string path, 
                             IFileSystem fileSystem, 
                             FileBasicInformation fileBasicInformation, 
                             FileStandardInformation fileStandardInformation,
                             ISMBCredential credential) : this(path, fileSystem)
        {
            if (fileBasicInformation.CreationTime.Time.HasValue)
            {
                _creationTime = fileBasicInformation.CreationTime.Time.Value;
                _creationTimeUtc = CreationTime.ToUniversalTime();
            }
            if (fileBasicInformation.LastAccessTime.Time.HasValue)
            {
                _lastAccessTime = fileBasicInformation.LastAccessTime.Time.Value;
                _lastAccessTimeUtc = LastAccessTime.ToUniversalTime();
            }
            if (fileBasicInformation.LastWriteTime.Time.HasValue)
            {
                _lastWriteTime = fileBasicInformation.LastWriteTime.Time.Value;
                _lastWriteTimeUtc = LastWriteTime.ToUniversalTime();
            }

            _attributes = (System.IO.FileAttributes)fileBasicInformation.FileAttributes;

            var pathUri = new Uri(path);
            var parentUri = pathUri.AbsoluteUri.EndsWith('/') ? new Uri(pathUri, "..") : new Uri(pathUri, ".");
            var parentPathString = parentUri.IsUnc ? parentUri.LocalPath : parentUri.AbsoluteUri;

            _directory = _dirInfoFactory.FromDirectoryName(parentPathString, credential);
            _directoryName = Directory?.Name;
            _exists = _file.Exists(path);
            _isReadOnly = fileBasicInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.ReadOnly);
            _length = fileStandardInformation.EndOfFile;
        }

        private IDirectoryInfo _directory;
        private string _directoryName;
        private bool _isReadOnly;
        private long _length;
        private System.IO.FileAttributes _attributes;
        private DateTime _creationTime;
        private DateTime _creationTimeUtc;
        private bool _exists;
        private string _fullName;
        private DateTime _lastAccessTime;
        private DateTime _lastAccessTimeUtc;
        private DateTime _lastWriteTime;
        private DateTime _lastWriteTimeUtc;

        public override IDirectoryInfo Directory { get => _directory; }
        public override string DirectoryName { get => _directoryName; }
        public override bool IsReadOnly { get => _isReadOnly; }
        public override long Length { get => _length; }
        public override System.IO.FileAttributes Attributes { get => _attributes; }
        public override DateTime CreationTime { get => _creationTime; }
        public override DateTime CreationTimeUtc { get => _creationTimeUtc; }
        public override bool Exists { get => _exists; }
        public override string FullName { get => _fullName; }
        public override DateTime LastAccessTime { get => _lastAccessTime; }
        public override DateTime LastAccessTimeUtc { get => _lastAccessTimeUtc; }
        public override DateTime LastWriteTime { get => _lastWriteTime; }
        public override DateTime LastWriteTimeUtc { get => _lastWriteTimeUtc; }

        public override StreamWriter AppendText()
        {
            return _file.AppendText(FullName);
        }

        public override IFileInfo CopyTo(string destFileName)
        {
            _file.Copy(FullName, destFileName);
            return _fileInfoFactory.FromFileName(destFileName);
        }

        public override IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            _file.Copy(FullName, destFileName, overwrite);
            return _fileInfoFactory.FromFileName(destFileName);
        }

        public override Stream Create()
        {
            var stream = _file.Create(FullName);
            _exists = true;
            return stream;
        }

        public override StreamWriter CreateText()
        {
            var streamWriter = _file.CreateText(FullName);
            _exists = true;
            return streamWriter;
        }

        public override void Delete()
        {
            _file.Delete(FullName);
            _exists = false;
        }

        public override FileSecurity GetAccessControl()
        {
            return _file.GetAccessControl(FullName);
        }

        public override FileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _file.GetAccessControl(FullName, includeSections);
        }

        public override void MoveTo(string destFileName)
        {
            _file.Move(FullName, destFileName);
        }

        public override Stream Open(FileMode mode)
        {
            var stream = _file.Open(FullName, mode);
            _exists = true;
            return stream;
        }

        public override Stream Open(FileMode mode, FileAccess access)
        {
            var stream = _file.Open(FullName, mode, access);
            _exists = true;
            return stream;
        }

        public override Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            var stream = _file.Open(FullName, mode, access, share);
            _exists = true;
            return stream;
        }

        public override Stream OpenRead()
        {
            var stream = _file.OpenRead(FullName);
            _exists = true;
            return stream;
        }

        public override StreamReader OpenText()
        {
            var streamReader = _file.OpenText(FullName);
            _exists = true;
            return streamReader;
        }

        public override Stream OpenWrite()
        {
            var stream = _file.OpenWrite(FullName);
            _exists = true;
            return stream;
        }

        public override void Refresh()
        {
            var fileInfo = _fileInfoFactory.FromFileName(FullName);

            _directory = fileInfo.Directory;
            _directoryName = fileInfo.DirectoryName;
            _isReadOnly = fileInfo.IsReadOnly;
            _length = fileInfo.Length;
            _attributes = fileInfo.Attributes;
            _creationTime = fileInfo.CreationTime;
            _creationTimeUtc = fileInfo.CreationTimeUtc;
            _exists = fileInfo.Exists;
            _fullName = fileInfo.FullName;
            _lastAccessTime = fileInfo.LastAccessTime;
            _lastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            _lastWriteTime = fileInfo.LastWriteTime;
            _lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
        }

        public override void SetAccessControl(FileSecurity fileSecurity)
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

        public override void Decrypt()
        {
            if (!FullName.IsSharePath())
            {
                base.Decrypt();
            }

            throw new NotImplementedException();
        }

        public override void Encrypt()
        {
            if(!FullName.IsSharePath())
            {
                base.Encrypt();
            }

            throw new NotImplementedException();
        }

        public override IFileInfo Replace(string destinationFilePath, string destinationBackupFilePath)
        {
            return Replace(destinationFilePath, destinationBackupFilePath, false);
        }

        public override IFileInfo Replace(string destinationFilePath, string destinationBackupFilePath, bool ignoreMetadataErrors)
        {
            if (string.IsNullOrEmpty(destinationFilePath))
            {
                throw new ArgumentNullException(nameof(destinationFilePath));
            }

            if(destinationBackupFilePath == string.Empty)
            {
                ///https://docs.microsoft.com/en-us/dotnet/api/system.io.fileinfo.replace?view=netcore-3.1
                throw new ArgumentNullException(nameof(destinationBackupFilePath), 
                                                $"Destination backup path cannot be empty. Pass null if you do not want to create backup of file being replaced.");
            }

            var path = FullName;
            
            if (!path.IsSharePath() && !destinationFilePath.IsSharePath())
            {
                return base.Replace(destinationFilePath, destinationBackupFilePath, ignoreMetadataErrors);
            }

            //Check if destination file exists, throw if doesnt
            if (!_file.Exists(destinationFilePath))
            {
                throw new FileNotFoundException($"Destination file {destinationFilePath} not found.");
            }

            // If backupPath is specified 
            // delete the backupfile if it exits
            // then copy destinatonFile to backupPath
            if (!string.IsNullOrEmpty(destinationBackupFilePath))
            {
                if(_file.Exists(destinationBackupFilePath))
                {
                   _file.Delete(destinationBackupFilePath);
                }

                _file.Copy(destinationFilePath, destinationBackupFilePath);
            }

            // Copy and overwrite destinationFile with current file
            // then delete original file
            _file.Copy(path, destinationFilePath, overwrite: true);
            _file.Delete(path);
            
            var replacedFile = _fileInfoFactory.FromFileName(destinationFilePath);
            return replacedFile;
        }
    }
}
