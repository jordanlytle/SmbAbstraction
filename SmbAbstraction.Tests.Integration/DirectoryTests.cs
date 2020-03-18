using Microsoft.Extensions.Configuration;
using System;
using Xunit;
using SmbAbstraction;
using System.Linq;
using System.IO;

namespace SmbAbstraction.Tests.Integration
{
    public class DirectoryTests : TestBase
    {
        private string createdTestDirectoryPath;

        public DirectoryTests() : base()
        {

        }

        public override void Dispose()
        {
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanCreateDirectoryInUncRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            createdTestDirectoryPath = Path.Combine(testShare.RootUncPath, $"test_directory-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            var directoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(FileSystem.Directory.Exists(createdTestDirectoryPath));

            FileSystem.Directory.Delete(createdTestDirectoryPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanCreateNestedDirectoryInUncRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            var parentDirectoryPath = Path.Combine(testShare.RootUncPath, $"test_directory_parent-{DateTime.Now.ToFileTimeUtc()}");
            createdTestDirectoryPath = Path.Combine(parentDirectoryPath, $"test_directory_child-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            var directoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(FileSystem.Directory.Exists(createdTestDirectoryPath));

            FileSystem.Directory.Delete(createdTestDirectoryPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanEnumerateFilesUncRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootUncPath, SMBCredentialProvider);

            var files = FileSystem.Directory.EnumerateFiles(testRootUncPath, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanEnumerateFilesSmbRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootSmbUri = Path.Combine(testShare.RootSmbUri);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootSmbUri, SMBCredentialProvider);

            var files = FileSystem.Directory.EnumerateFiles(testRootSmbUri, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }
    }
}
