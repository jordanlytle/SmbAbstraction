using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace SmbAbstraction.Tests.Integration.DirectoryInfo
{
    public abstract class DirectoryInfoTests
    {
        readonly TestFixture _fixture;
        readonly IFileSystem _fileSystem;

        public DirectoryInfoTests(TestFixture fixture)
        {
            _fixture = fixture;
            _fileSystem = _fixture.FileSystem;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void MoveLocalDirectoryToUncShare()
        {
            var credentials = _fixture.ShareCredentials;
            var share = _fixture.Shares.First();
            var rootPath = share.GetRootPath(_fixture.PathType);
            var directory = _fileSystem.Path.Combine(rootPath, share.Directories.First());
            var newDirectory = _fileSystem.Path.Combine(directory, $"{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var createDirectoryPath = _fileSystem.Path.Combine(rootPath, $"test-move-local-directory-{DateTime.Now.ToFileTimeUtc()}");
            var directoryInfo = _fileSystem.Directory.CreateDirectory(createDirectoryPath);
            
            directoryInfo.MoveTo(newDirectory);

            Assert.True(_fileSystem.Directory.Exists(newDirectory));

            _fileSystem.Directory.Delete(directoryInfo.FullName);
            _fileSystem.Directory.Delete(newDirectory);
        }
    }
}
