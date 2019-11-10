using System;
namespace System.IO.Abstractions.SMB
{
    public class SMBDriveInfo : IDriveInfo
    {
        public SMBDriveInfo(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }

        public long AvailableFreeSpace => throw new NotImplementedException();

        public string DriveFormat => throw new NotImplementedException();

        public DriveType DriveType => throw new NotImplementedException();

        public bool IsReady => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public IDirectoryInfo RootDirectory => throw new NotImplementedException();

        public long TotalFreeSpace => throw new NotImplementedException();

        public long TotalSize => throw new NotImplementedException();

        public string VolumeLabel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
