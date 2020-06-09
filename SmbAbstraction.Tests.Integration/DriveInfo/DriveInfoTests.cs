using System.IO.Abstractions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SmbAbstraction.Tests.Integration.DriveInfo
{
    public abstract class DriveInfoTests
    {
        readonly TestFixture _fixture;
        private IFileSystem _fileSystem;

        public DriveInfoTests(TestFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture.WithLoggerFactory(outputHelper.ToLoggerFactory());
            _fileSystem = _fixture.FileSystem;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FromDriveName_ReturnsNotNull()
        {
            var credentials = _fixture.ShareCredentials;

            _fixture.SMBCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shareInfo = smbDriveInfoFactory.FromDriveName(_fixture.ShareName);

            Assert.NotNull(shareInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FromDriveName_WithFileName_ReturnsNotNull()
        {
            var credentials = _fixture.ShareCredentials;
            var fileName = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());

            _fixture.SMBCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, fileName, _fixture.SMBCredentialProvider));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shareInfo = smbDriveInfoFactory.FromDriveName(fileName);

            Assert.NotNull(shareInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FromDriveName_WithDirectory_ReturnsNotNull()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());

            _fixture.SMBCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shareInfo = smbDriveInfoFactory.FromDriveName(directory);

            Assert.NotNull(shareInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetDrives_WithCredentials_ReturnsNotNull()
        {
            var credentials = _fixture.ShareCredentials;
            
            _fixture.SMBCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shares = smbDriveInfoFactory.GetDrives();

            Assert.NotNull(shares);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetDrives_WithNoCredentials_ReturnsNotNull()
        {
            var credentials = _fixture.ShareCredentials;

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shares = smbDriveInfoFactory.GetDrives();

            Assert.NotNull(shares);
        }
    }
}
