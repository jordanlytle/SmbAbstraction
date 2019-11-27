using SmbLibraryStd;
using System;
namespace System.IO.Abstractions.SMB
{
    public class SMBDriveInfo : IDriveInfo
    {
        private SMBDirectoryInfoFactory _dirInfoFactory => FileSystem.DirectoryInfo as SMBDirectoryInfoFactory;
        private string _volumeLabel;

        public SMBDriveInfo(string path, IFileSystem fileSystem, SMBFileSystemInformation smbFileSystemInformation, ISMBCredential credential)
        {
            FileSystem = fileSystem;
            AvailableFreeSpace = smbFileSystemInformation.SizeInformation.CallerAvailableAllocationUnits;
            DriveFormat = smbFileSystemInformation.AttributeInformation.FileSystemName;
            Name = path.ShareName();
            RootDirectory = _dirInfoFactory.FromDirectoryName(path, credential);
            TotalFreeSpace = smbFileSystemInformation.SizeInformation.ActualAvailableAllocationUnits;
            TotalSize = smbFileSystemInformation.SizeInformation.TotalAllocationUnits;
            _volumeLabel = smbFileSystemInformation.VolumeInformation.VolumeLabel;
        }

        public IFileSystem FileSystem { get; }

        public long AvailableFreeSpace { get; }

        public string DriveFormat { get; }

        public DriveType DriveType => DriveType.Network;

        public bool IsReady => throw new NotImplementedException();

        public string Name { get; }

        public IDirectoryInfo RootDirectory { get; }

        public long TotalFreeSpace { get; }

        public long TotalSize { get; }

        public string VolumeLabel { get => _volumeLabel; set => throw new NotSupportedException(); }
    }
}
