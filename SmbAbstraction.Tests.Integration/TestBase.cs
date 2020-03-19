using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace SmbAbstraction.Tests.Integration
{
    public class TestBase : IDisposable
    {
        private IntegrationTestSettings _settings = new IntegrationTestSettings();

        private readonly ISMBClientFactory _clientFactory;

        public TestBase()
        {
            _settings.Initialize();
            SMBCredentialProvider = new SMBCredentialProvider();
            _clientFactory = new SMB2ClientFactory();
            FileSystem = new SMBFileSystem(_clientFactory, SMBCredentialProvider);
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

        public string LocalTempDirectory { 
            get
            {
                if(!string.IsNullOrEmpty(_settings.LocalTempFolder))
                {
                    return _settings.LocalTempFolder;
                }

                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $@"C:\temp";
                }
                else
                {
                    return $@"{Environment.GetEnvironmentVariable("HOME")}/";
                }
            } 
        }

        public virtual void Dispose()
        {

        }
    }
}
