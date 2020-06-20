using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace SmbAbstraction.Tests.Integration.File
{
    public abstract class FileTests
    {
        readonly TestFixture _fixture;
        private IFileSystem _fileSystem;

        public FileTests(TestFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture.WithLoggerFactory(outputHelper.ToLoggerFactory());
            _fileSystem = _fixture.FileSystem;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckDeleteCompletes()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.LocalTempDirectory, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            var fileInfo = _fileSystem.FileInfo.FromFileName(tempFilePath);
            fileInfo = fileInfo.CopyTo(_fileSystem.Path.Combine(directory, tempFileName));

            fileInfo.Delete();

            _fileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestExistsForExistingFile()
        {
            var credentials = _fixture.ShareCredentials;
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);

            Assert.True(_fileSystem.File.Exists(filePath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestExistsForNonExistingFile()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);

            Assert.False(_fileSystem.File.Exists(tempFilePath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestExistsForDirectory()
        {
            var credentials = _fixture.ShareCredentials;
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);

            Assert.False(_fileSystem.File.Exists(filePath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestMove()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            var moveToFilePath = _fileSystem.Path.Combine(directory, tempFileName);

            _fileSystem.File.Move(tempFilePath, moveToFilePath);

            Assert.True(_fileSystem.File.Exists(moveToFilePath));
            Assert.False(_fileSystem.File.Exists(tempFilePath));

        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestMove_WhereSourceIsLocal()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.LocalTempDirectory, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            var moveToFilePath = _fileSystem.Path.Combine(directory, tempFileName);

            _fileSystem.File.Move(tempFilePath, moveToFilePath);

            Assert.True(_fileSystem.File.Exists(moveToFilePath));
            Assert.False(_fileSystem.File.Exists(tempFilePath));

        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestMoveBetweenFileSystems_WhereSourceIsShare()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            var moveToFilePath = _fileSystem.Path.Combine(directory, tempFileName);

            _fileSystem.File.Move(tempFilePath, moveToFilePath);

            Assert.True(_fileSystem.File.Exists(moveToFilePath));
            Assert.False(_fileSystem.File.Exists(tempFilePath));

            _fileSystem.File.Delete(moveToFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestCreateFile()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            using var fileStream = _fileSystem.File.Create(tempFilePath);
            fileStream.Close();

            _fileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestFileOpenOnExistingFile()
        {
            var credentials = _fixture.ShareCredentials;
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);

            using var fileStream = _fileSystem.File.Open(filePath, FileMode.Open);
            fileStream.Close();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestFileOpenOnCreatedFile()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            using var fileStream = _fileSystem.File.Open(tempFilePath, FileMode.Open);
            fileStream.Close();

            _fileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestFileOpen_FileModeCreate()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            using var fileStream = _fileSystem.File.Open(tempFilePath, FileMode.Create);
            fileStream.Close();

            _fileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestFileOpen_FileModeOpenOrCreate()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            using var fileStream = _fileSystem.File.Open(tempFilePath, FileMode.OpenOrCreate);
            fileStream.Close();

            _fileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanSetCreationTime()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var tempFilePath = _fileSystem.Path.Combine(_fixture.RootPath, tempFileName);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, _fixture.RootPath, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var stream = _fileSystem.File.Create(tempFilePath))
                {
                }
            }

            _fileSystem.File.SetCreationTime(tempFilePath, DateTime.Now.AddMinutes(10));

            _fileSystem.File.Delete(tempFilePath);
        }
    }
}
