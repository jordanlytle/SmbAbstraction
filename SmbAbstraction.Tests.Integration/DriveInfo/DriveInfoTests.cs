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
            var share = _fixture.Shares.FirstOrDefault();
            var rootPath = share.GetRootPath(_fixture.PathType);

            _fixture.SMBCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, rootPath, _fixture.SMBCredentialProvider));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shareInfo = smbDriveInfoFactory.FromDriveName(share.ShareName);

            Assert.NotNull(shareInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetDrives_ReturnsNotNull()
        {
            var credentials = _fixture.ShareCredentials;
            var share = _fixture.Shares.FirstOrDefault();
            var rootPath = share.GetRootPath(_fixture.PathType);

            _fixture.SMBCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, rootPath, _fixture.SMBCredentialProvider));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _fixture.SMBCredentialProvider, _fixture.SMBClientFactory, 65536);

            var shares = smbDriveInfoFactory.GetDrives();

            Assert.NotNull(shares);
        }
    }
}
