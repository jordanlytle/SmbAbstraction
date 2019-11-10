using System;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileStreamFactory : IFileStreamFactory
    {
        public SMBFileStreamFactory()
        {
        }

        public Stream Create(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public Stream Create(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            throw new NotImplementedException();
        }

        public Stream Create(SafeFileHandle handle, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public Stream Create(SafeFileHandle handle, FileAccess access, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public Stream Create(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
        {
            throw new NotImplementedException();
        }
    }
}
