using SMBLibrary;
using System;
using System.IO;
using System.IO.Abstractions;

namespace SmbAbstraction
{
    public class SMBDriveInfo : IDriveInfo
    {
        private SMBDirectoryInfoFactory _dirInfoFactory => FileSystem.DirectoryInfo as SMBDirectoryInfoFactory;
        private string _volumeLabel;

        public SMBDriveInfo(string path, IFileSystem fileSystem, SMBFileSystemInformation smbFileSystemInformation, ISMBCredential credential)
        {
            FileSystem = fileSystem;
            DriveFormat = smbFileSystemInformation.AttributeInformation?.FileSystemName;
            Name = path.ShareName();
            RootDirectory = _dirInfoFactory.FromDirectoryName(path, credential);
            var actualAvailableAllocationUnits = smbFileSystemInformation.SizeInformation.ActualAvailableAllocationUnits;
            var sectorsPerUnit = smbFileSystemInformation.SizeInformation.SectorsPerAllocationUnit;
            var bytesPerSector = smbFileSystemInformation.SizeInformation.BytesPerSector;
            var totalAllocationUnits = smbFileSystemInformation.SizeInformation.TotalAllocationUnits;
            var availableAllocationUnits = smbFileSystemInformation.SizeInformation.CallerAvailableAllocationUnits;  

            AvailableFreeSpace = availableAllocationUnits * sectorsPerUnit * bytesPerSector;
            TotalFreeSpace = actualAvailableAllocationUnits * sectorsPerUnit * bytesPerSector;
            TotalSize = totalAllocationUnits * sectorsPerUnit * bytesPerSector;
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
