using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace SmbAbstraction.Tests.Integration
{
    public abstract class TestFixture : IDisposable
    {
        private readonly IntegrationTestSettings _settings = new IntegrationTestSettings();

        public TestFixture()
        {
            _settings.Initialize();
            SMBCredentialProvider = new SMBCredentialProvider();
            SMBClientFactory = new SMB2ClientFactory();
            FileSystem = new SMBFileSystem(SMBClientFactory, SMBCredentialProvider);
        }

        public IntegrationTestSettings TestSettings
        {
            get
            {
                return _settings;
            }
        }

        public IFileSystem FileSystem { get; }

        public ISMBCredentialProvider SMBCredentialProvider { get; }
        
        public ISMBClientFactory SMBClientFactory { get; }


        public string LocalTempDirectory {
            get
            {
                if (!string.IsNullOrEmpty(_settings.LocalTempFolder))
                {
                    return _settings.LocalTempFolder;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $@"C:\temp";
                }
                else
                {
                    return $@"{Environment.GetEnvironmentVariable("HOME")}/";
                }
            }
        }

        public ShareCredentials ShareCredentials { get => _settings.ShareCredentials; }
        public List<Share> Shares { get => _settings.Shares; }

        public abstract PathType PathType { get; }


        public virtual void Dispose()
        {

        }
    }
}
