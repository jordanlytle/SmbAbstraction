using System;
namespace System.IO.Abstractions.SMB
{
    public class SMBDriveInfoFactory : IDriveInfoFactory
    {
        public SMBDriveInfoFactory()
        {
        }

        public IDriveInfo FromDriveName(string driveName)
        {
            throw new NotImplementedException();
        }

        public IDriveInfo[] GetDrives()
        {
            throw new NotImplementedException();
        }
    }
}
