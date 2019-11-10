using System;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileInfoFactory : IFileInfoFactory
    {
        public SMBFileInfoFactory()
        {
        }

        public IFileInfo FromFileName(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
