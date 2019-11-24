using SmbLibraryStd;
using SmbLibraryStd.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileSystemInformation
    {
        public FileFsVolumeInformation VolumeInformation { get; }
        public FileFsDeviceInformation DeviceInformation { get; }
        public FileFsFullSizeInformation SizeInformation { get; }
        public FileFsAttributeInformation AttributeInformation { get; }
        public FileFsControlInformation ControlInformation { get; }
        public FileFsSectorSizeInformation SectorSizeInformation { get; }

        public SMBFileSystemInformation(ISMBFileStore fileStore, string path)
        {
            var shareName = path.ShareName();
            var relativePath = path.RelativeSharePath();

            NTStatus status = NTStatus.STATUS_SUCCESS;

            status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, relativePath,
                                      AccessMask.GENERIC_READ, 0, ShareAccess.Read, CreateDisposition.FILE_OPEN,
                                      CreateOptions.FILE_DIRECTORY_FILE, null);

            if (status == NTStatus.STATUS_SUCCESS)
            {
                fileStore.GetFileSystemInformation(out var fileFsVolumeInformation, FileSystemInformationClass.FileFsVolumeInformation);
                fileStore.GetFileSystemInformation(out var fileFsDeviceInformation, FileSystemInformationClass.FileFsDeviceInformation);
                fileStore.GetFileSystemInformation(out var fileFsFullSizeInformation, FileSystemInformationClass.FileFsFullSizeInformation);
                fileStore.GetFileSystemInformation(out var fileFsAttributeInformation, FileSystemInformationClass.FileFsAttributeInformation);
                fileStore.GetFileSystemInformation(out var fileFsControlInformation, FileSystemInformationClass.FileFsControlInformation);
                fileStore.GetFileSystemInformation(out var fileFsSectorSizeInformation, FileSystemInformationClass.FileFsSectorSizeInformation);

                VolumeInformation = (FileFsVolumeInformation)fileFsVolumeInformation;
                DeviceInformation = (FileFsDeviceInformation)fileFsDeviceInformation;
                SizeInformation = (FileFsFullSizeInformation)fileFsFullSizeInformation;
                AttributeInformation = (FileFsAttributeInformation)fileFsAttributeInformation;
                ControlInformation = (FileFsControlInformation)fileFsControlInformation;
                SectorSizeInformation = (FileFsSectorSizeInformation)fileFsSectorSizeInformation;
            }
        }
    }
}
