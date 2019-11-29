using System;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Abstractions.SMB
{
    public class SMBFileStreamFactory : IFileStreamFactory
    {
        private readonly IFileSystem _fileSystem;
        private SMBFile _smbFile => _fileSystem.File as SMBFile;

        public SMBFileStreamFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Stream Create(string path, FileMode mode)
        {
            if (path.IsSmbPath())
            {
                return new FileStream(path, mode);
            }

            return _fileSystem.File.Open(path, mode);
        }

        public Stream Create(string path, FileMode mode, FileAccess access)
        {
            if (path.IsSmbPath())
            {
                return new FileStream(path, mode, access);
            }

            return _fileSystem.File.Open(path, mode, access);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (path.IsSmbPath())
            {
                return new FileStream(path, mode, access, share);
            }

            return _fileSystem.File.Open(path, mode, access, share);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            if (path.IsSmbPath())
            {
                return new FileStream(path, mode, access, share, bufferSize);
            }

            return new BufferedStream(_fileSystem.File.Open(path, mode, access, share), bufferSize);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
            if (path.IsSmbPath())
            {
                return new FileStream(path, mode, access, share, bufferSize, options);
            }

            return new BufferedStream(_smbFile.Open(path, mode, access, share, options, null), bufferSize);
        }

        public Stream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            if (path.IsSmbPath())
            {
                return new FileStream(path, mode, access, share, bufferSize, useAsync);
            }

            if (useAsync == false)
            {
                return new BufferedStream(_fileSystem.File.Open(path, mode, access, share), bufferSize);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public Stream Create(SafeFileHandle handle, FileAccess access)
        {
            throw new NotSupportedException();
        }

        public Stream Create(SafeFileHandle handle, FileAccess access, int bufferSize)
        {
            throw new NotSupportedException();
        }

        public Stream Create(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
        {
            throw new NotSupportedException();
        }
    }
}
