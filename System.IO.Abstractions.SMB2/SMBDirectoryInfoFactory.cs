using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace System.IO.Abstractions.SMB
{
    public class SMBDirectoryInfoFactory : IDirectoryInfoFactory
    {
        public SMBDirectoryInfoFactory()
        {
        }

        public IDirectoryInfo FromDirectoryName(string directoryName)
        {
            throw new NotImplementedException();
        }
    }
}
