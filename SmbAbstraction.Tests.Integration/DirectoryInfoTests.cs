using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace SmbAbstraction.Tests.Integration
{
    public class DirectoryInfoTests : TestBase
    {
        public DirectoryInfoTests() : base()
        {
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void MoveLocalDirectoryToUncShare()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = Path.Combine(testRootUncPath, testShare.Directories.First());
            var newUncDirectory = Path.Combine(uncDirectory, $"{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            var directoryInfo = FileSystem.Directory.CreateDirectory(@$"{Path.Combine(testRootUncPath, $"test-move-local-directory-{DateTime.Now.ToFileTimeUtc()}")}");
            

            directoryInfo.MoveTo(newUncDirectory);

            Assert.True(FileSystem.Directory.Exists(newUncDirectory));

            FileSystem.Directory.Delete(directoryInfo.FullName);
            FileSystem.Directory.Delete(newUncDirectory);
        }
    }
}
