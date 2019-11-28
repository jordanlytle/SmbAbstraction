using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace System.IO.Abstractions.SMB.Tests.Integration
{
    public class DriveInfoTests : TestBase
    {
        private IFileSystem _fileSystem;
        private ISMBCredentialProvider _smbCredentialProvider;
        private ISMBClientFactory _smbClientFactory;

        public DriveInfoTests(): base()
        {
            _smbCredentialProvider = new SMBCredentialProvider();
            _smbClientFactory = new SMB2ClientFactory();
            _fileSystem = new SMBFileSystem(_smbClientFactory, _smbCredentialProvider);
        }

        public override void Dispose()
        {

        }


        [Fact]
        [Trait("Category", "Integration")]
        public void FromDriveName_ReturnsNotNull_ForSmbUri()
        {
            var credentials = TestSettings.ShareCredentials;
            var share = TestSettings.Shares.FirstOrDefault();

            _smbCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, share.RootSmbUri));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _smbCredentialProvider, _smbClientFactory);

            var shareInfo = smbDriveInfoFactory.FromDriveName(share.ShareName);

            Assert.NotNull(shareInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FromDriveName_ReturnsNotNull_ForUncPath()
        {
            var credentials = TestSettings.ShareCredentials;
            var share = TestSettings.Shares.FirstOrDefault();

            _smbCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, share.RootUncPath));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _smbCredentialProvider, _smbClientFactory);

            var shareInfo = smbDriveInfoFactory.FromDriveName(share.ShareName);

            Assert.NotNull(shareInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetDrives_ReturnsNotNull_ForSmbUri()
        {
            var credentials = TestSettings.ShareCredentials;
            var share = TestSettings.Shares.FirstOrDefault();

            _smbCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, share.RootSmbUri));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _smbCredentialProvider, _smbClientFactory);

            var shares = smbDriveInfoFactory.GetDrives();

            Assert.NotNull(shares);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetDrives_ReturnsNotNull_ForUncPath()
        {
            var credentials = TestSettings.ShareCredentials;
            var share = TestSettings.Shares.FirstOrDefault();

            _smbCredentialProvider.AddSMBCredential(new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, share.RootUncPath));

            var smbDriveInfoFactory = new SMBDriveInfoFactory(_fileSystem, _smbCredentialProvider, _smbClientFactory);
            
            var shares = smbDriveInfoFactory.GetDrives();

            Assert.NotNull(shares);
        }

    }
}
