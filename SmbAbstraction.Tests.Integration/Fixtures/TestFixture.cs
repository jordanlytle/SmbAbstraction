using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace SmbAbstraction.Tests.Integration
{
    public abstract class TestFixture : IDisposable
    {
        public TestFixture()
        {
            SMBCredentialProvider = new SMBCredentialProvider();
            SMBClientFactory = new SMB2ClientFactory();
            FileSystem = new SMBFileSystem(SMBClientFactory, SMBCredentialProvider);
        }

        public IFileSystem FileSystem { get; }

        public ISMBCredentialProvider SMBCredentialProvider { get; }

        public ISMBClientFactory SMBClientFactory { get; }

      
        public abstract string LocalTempDirectory { get; }
        public abstract ShareCredentials ShareCredentials { get; }
        public abstract string ShareName { get; }
        public abstract string RootPath { get; }
        public abstract List<string> Files { get; }
        public abstract List<string> Directories { get; }
        public abstract PathType PathType { get; }


        public virtual void Dispose()
        {

        }
    }
}
