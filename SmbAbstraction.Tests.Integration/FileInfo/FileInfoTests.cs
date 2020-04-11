using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Xunit;

namespace SmbAbstraction.Tests.Integration.FileInfo
{
    public abstract class FileInfoTests
    {
        readonly TestFixture _fixture;
        readonly IFileSystem _fileSystem;

        public FileInfoTests(TestFixture fixture)
        {
            _fixture = fixture;
            _fileSystem = _fixture.FileSystem;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CanCreateFileInfo()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var fileInfo = _fileSystem.FileInfo.FromFileName(filePath);

            Assert.NotNull(fileInfo);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CopyFromLocalDirectoryToShareDirectory()
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
            
            Assert.True(fileInfo.Exists);

            _fileSystem.File.Delete(tempFilePath);
            _fileSystem.File.Delete(fileInfo.FullName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFileSize()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.LocalTempDirectory, tempFileName);

            var byteArray = new byte[100];

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if (!_fileSystem.File.Exists(tempFilePath))
            {
                using (var stream = _fileSystem.File.Create(tempFilePath))
                {
                    stream.Write(byteArray, 0, 100);
                }
            }

            var fileInfo = _fileSystem.FileInfo.FromFileName(tempFilePath);
            var fileSize = fileInfo.Length;

            var destinationFilePath = _fileSystem.Path.Combine(directory, tempFileName);
            fileInfo = fileInfo.CopyTo(destinationFilePath);
            
            Assert.True(fileInfo.Exists);
            Assert.Equal(fileSize, fileInfo.Length);

            _fileSystem.File.Delete(fileInfo.FullName);
            _fileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFileExists()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var exists = _fileSystem.FileInfo.FromFileName(filePath).Exists;

            Assert.True(exists);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFileExtensionMatches()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());
            var fileExtension = _fileSystem.Path.GetExtension(filePath);

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var extenstion = _fileSystem.FileInfo.FromFileName(filePath).Extension;

            Assert.Equal(fileExtension, extenstion);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFullNameMatches()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var filePath = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Files.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var fullName = _fileSystem.FileInfo.FromFileName(filePath).FullName;

            Assert.Equal(filePath, fullName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckReplaceWithBackup()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var originalFileTime = DateTime.Now.ToFileTimeUtc();
            var originalFilePath = _fileSystem.Path.Combine(directory, $"replace-file-{originalFileTime}.txt");
            var originalFileBackupPath = _fileSystem.Path.Combine(directory, $"replace-file-{originalFileTime}.bak");

           if (!_fileSystem.File.Exists(originalFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(originalFilePath)))
                {
                    streamWriter.WriteLine($"{originalFileTime}");
                }
            }

            var newFileTime = DateTime.Now.ToFileTimeUtc();
            var newFilePath = _fileSystem.Path.Combine(_fixture.RootPath, $"replace-file-{newFileTime}.txt");

            if (!_fileSystem.File.Exists(newFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(newFilePath)))
                {
                    streamWriter.WriteLine($"{newFileTime}");
                }
            }

            var newFileInfo = _fileSystem.FileInfo.FromFileName(newFilePath);

            newFileInfo = newFileInfo.Replace(originalFilePath, originalFileBackupPath);

            Assert.Equal(originalFilePath, newFileInfo.FullName);
            Assert.False(_fileSystem.File.Exists(newFilePath));
            Assert.True(_fileSystem.File.Exists(originalFileBackupPath));

            using (var streamReader = new StreamReader(_fileSystem.File.OpenRead(newFileInfo.FullName)))
            {
                var line = streamReader.ReadLine();

                Assert.Equal(newFileTime.ToString(), line);
            }

            _fileSystem.File.Delete(originalFilePath);
            _fileSystem.File.Delete(originalFileBackupPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckReplaceWithoutBackup()
        {
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            var originalFileTime = DateTime.Now.ToFileTimeUtc();
            var originalFilePath = _fileSystem.Path.Combine(directory, $"replace-file-{originalFileTime}.txt");

            if (!_fileSystem.File.Exists(originalFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(originalFilePath)))
                {
                    streamWriter.WriteLine($"{originalFileTime}");
                }
            }

            var newFileTime = DateTime.Now.ToFileTimeUtc();
            var newFilePath = _fileSystem.Path.Combine(_fixture.RootPath, $"replace-file-{newFileTime}.txt");

            if (!_fileSystem.File.Exists(newFilePath))
            {
                using (var streamWriter = new StreamWriter(_fileSystem.File.Create(newFilePath)))
                {
                    streamWriter.WriteLine($"{newFileTime}");
                }
            }

            var newFileInfo = _fileSystem.FileInfo.FromFileName(newFilePath);

            newFileInfo = newFileInfo.Replace(originalFilePath, null);

            Assert.Equal(originalFilePath, newFileInfo.FullName);
            Assert.False(_fileSystem.File.Exists(newFilePath));

            using (var streamReader = new StreamReader(_fileSystem.File.OpenRead(newFileInfo.FullName)))
            {
                var line = streamReader.ReadLine();

                Assert.Equal(newFileTime.ToString(), line);
            }

            _fileSystem.File.Delete(originalFilePath);
        }
    }
}
