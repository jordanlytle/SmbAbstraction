using System;
namespace System.IO.Abstractions.SMB
{
    public class SMBFileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        public SMBFileSystemWatcherFactory()
        {
        }

        public IFileSystemWatcher CreateNew()
        {
            throw new NotImplementedException();
        }

        public IFileSystemWatcher FromPath(string path)
        {
            throw new NotImplementedException();
        }
    }
}
