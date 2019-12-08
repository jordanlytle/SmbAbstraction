using System;
using System.IO.Abstractions;

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

        public virtual void Dispose()
        {

        }
    }
}
