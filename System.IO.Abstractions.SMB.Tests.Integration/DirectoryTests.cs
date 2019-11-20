using Microsoft.Extensions.Configuration;
using System;
using Xunit;
using System.IO.Abstractions.SMB;
using System.Linq;

namespace System.IO.Abstractions.SMB.Tests.Integration
{
    public class DirectoryTests
    {
        private static readonly IntegrationTestSettings _settings = new IntegrationTestSettings();

        private readonly ISMBCredentialProvider _credentialProvider;
        private readonly ISMBClientFactory _clientFactory;
        private readonly IFileSystem _fileSystem;

        public DirectoryTests()
        {
            _settings.Initialize();

            _credentialProvider = new SMBCredentialProvider();
            _clientFactory = new SMB2ClientFactory();
            _fileSystem = new SMBFileSystem(_clientFactory, _credentialProvider);
        }

        [Fact]
        public void CanEnumerateFilesUncRootDirectory()
        {
            var testCredentials = _settings.ShareCredentials;
            var testShare = _settings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootUncPath, _credentialProvider);

            var files = _fileSystem.Directory.EnumerateFiles(testRootUncPath, "*").ToList();

            Assert.True(files.Count > 0);
        }

        [Fact]
        public void CanEnumerateFilesSmbRootDirectory()
        {
            var testCredentials = _settings.ShareCredentials;
            var testShare = _settings.Shares.First();
            var testRootSmbUri = Path.Combine(testShare.RootSmbUri);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootSmbUri, _credentialProvider);

            var files = _fileSystem.Directory.EnumerateFiles(testRootSmbUri, "*").ToList();

            Assert.True(files.Count > 0);
        }
    }
}
