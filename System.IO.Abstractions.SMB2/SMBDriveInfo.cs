using SmbLibraryStd;
using System;
namespace System.IO.Abstractions.SMB
{
    public class SMBDriveInfo : IDriveInfo
    {
        private readonly SMBFileSystemInformation _smbFileSystemInformation;
        private SMBDirectoryInfoFactory _dirInfoFactory => FileSystem.DirectoryInfo as SMBDirectoryInfoFactory;
        public SMBDriveInfo(string path, IFileSystem fileSystem, SMBFileSystemInformation smbFileSystemInformation, ISMBCredential credential)
        {
            FileSystem = fileSystem;
            _smbFileSystemInformation = smbFileSystemInformation;
            Name = path.ShareName();
            RootDirectory = _dirInfoFactory.FromDirectoryName(path, credential);
        }

        public IFileSystem FileSystem { get; }

        public long AvailableFreeSpace => _smbFileSystemInformation.SizeInformation.CallerAvailableAllocationUnits;

        public string DriveFormat => _smbFileSystemInformation.AttributeInformation.FileSystemName;

        public DriveType DriveType => DriveType.Network;

        public bool IsReady => throw new NotImplementedException();

        public string Name { get; }

        public IDirectoryInfo RootDirectory { get; }

        public long TotalFreeSpace => _smbFileSystemInformation.SizeInformation.ActualAvailableAllocationUnits;

        public long TotalSize => _smbFileSystemInformation.SizeInformation.TotalAllocationUnits;

        public string VolumeLabel { get => _smbFileSystemInformation.VolumeInformation.VolumeLabel; set => throw new NotSupportedException(); }
    }
}
