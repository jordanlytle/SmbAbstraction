using System.IO.Abstractions;
using System.Linq;
using Xunit;

namespace SmbAbstraction.Tests.Integration.DriveInfo
{
    public abstract class DriveInfoTests
    {
        readonly TestFixture _fixture;
        private IFileSystem _fileSystem;

        public DriveInfoTests(TestFixture fixture)
        {
            _fixture = fixture;
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
