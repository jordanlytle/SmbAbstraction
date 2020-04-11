using Microsoft.Extensions.Configuration;
using System;
using Xunit;
using SmbAbstraction;
using System.Linq;
using System.IO;
using System.IO.Abstractions;

namespace SmbAbstraction.Tests.Integration.Directory
{
    public abstract class DirectoryTests
    {
        private string createdTestDirectoryPath;
        readonly TestFixture _fixture;
        readonly IFileSystem _fileSystem;

        public DirectoryTests(TestFixture fixture)
        {
            _fixture = fixture;
            _fileSystem = _fixture.FileSystem;
        }

        public void Dispose()
        {
            _fileSystem.Directory.Delete(createdTestDirectoryPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanCreateDirectoryInRootDirectory()
        {
            var credentials = _fixture.ShareCredentials;

            createdTestDirectoryPath = _fileSystem.Path.Combine(_fixture.RootPath, $"test_directory-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, createdTestDirectoryPath, _fixture.SMBCredentialProvider);

            var directoryInfo = _fileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(_fileSystem.Directory.Exists(createdTestDirectoryPath));

            
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanCreateNestedDirectoryInRootDirectory()
        {
            var credentials = _fixture.ShareCredentials;

            var parentDirectoryPath = _fileSystem.Path.Combine(_fixture.RootPath, $"test_directory_parent-{DateTime.Now.ToFileTimeUtc()}");
            createdTestDirectoryPath = _fileSystem.Path.Combine(parentDirectoryPath, $"test_directory_child-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, createdTestDirectoryPath, _fixture.SMBCredentialProvider);

            var directoryInfo = _fileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(_fileSystem.Directory.Exists(createdTestDirectoryPath));

            _fileSystem.Directory.Delete(createdTestDirectoryPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanEnumerateFilesRootDirectory()
        {
            var credentials = _fixture.ShareCredentials;

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);

            var files = _fileSystem.Directory.EnumerateFiles(_fixture.RootPath, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckDirectoryExists()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);
            
            Assert.True(_fileSystem.Directory.Exists(directory));
        }
    }
}
