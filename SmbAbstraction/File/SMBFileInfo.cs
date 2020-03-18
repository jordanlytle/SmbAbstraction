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
        private SMBFile _file => FileSystem.File as SMBFile;
        private SMBFileInfoFactory _fileInfoFactory => FileSystem.FileInfo as SMBFileInfoFactory;
        private SMBDirectoryInfoFactory _dirInfoFactory => FileSystem.DirectoryInfo as SMBDirectoryInfoFactory;
        public IFileSystem FileSystem { get; }

        public SMBFileInfo(string path, 
                           IFileSystem fileSystem): base(new FileSystem(), new FileInfo(path))
        {
            _fullName = path;
            FileSystem = fileSystem;
        }

        internal SMBFileInfo(FileInfo fileInfo, 
                             IFileSystem fileSystem) : this(fileInfo.FullName, fileSystem/*, smbClientFactory, credentialProvider, maxBufferSize*/)
        {
            CreationTime = fileInfo.CreationTime;
            CreationTimeUtc = fileInfo.CreationTimeUtc;
            LastAccessTime = fileInfo.LastAccessTime;
            LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            LastWriteTime = fileInfo.LastWriteTime;
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            Attributes = fileInfo.Attributes;
            _directory = new DirectoryInfoWrapper(fileSystem, fileInfo.Directory);
            _directoryName = fileInfo.DirectoryName;
            _exists = fileInfo.Exists;
            IsReadOnly = fileInfo.IsReadOnly;
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
            var parentPathString = parentUri.IsUnc ? parentUri.LocalPath : parentUri.AbsoluteUri;

            _directory = _dirInfoFactory.FromDirectoryName(parentPathString, credential);
            _directoryName = Directory?.Name;
            _exists = _file.Exists(path);
            IsReadOnly = fileBasicInformation.FileAttributes.HasFlag(SMBLibrary.FileAttributes.ReadOnly);
            _length = fileStandardInformation.EndOfFile;
        }

        private IDirectoryInfo _directory;
        public override IDirectoryInfo Directory { get => _directory; }

        private string _directoryName;
        public override string DirectoryName { get => _directoryName; }

        private long _length;
        public override long Length { get => _length; }

        private bool _exists;
        public override bool Exists { get => _exists; }


        private string _fullName;
        public override string FullName { get => _fullName; }

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
            IsReadOnly = fileInfo.IsReadOnly;
            _length = fileInfo.Length;
            Attributes = fileInfo.Attributes;
            CreationTime = fileInfo.CreationTime;
            CreationTimeUtc = fileInfo.CreationTimeUtc;
            _exists = fileInfo.Exists;
            _fullName = fileInfo.FullName;
            LastAccessTime = fileInfo.LastAccessTime;
            LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            LastWriteTime = fileInfo.LastWriteTime;
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
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
            if (!FullName.IsSmbPath())
            {
                base.Decrypt();
            }

            throw new NotImplementedException();
        }

        public override void Encrypt()
        {
            if(!FullName.IsSmbPath())
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

            var path = FullName;
            
            if (!path.IsSmbPath() && !destinationFilePath.IsSmbPath())
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
