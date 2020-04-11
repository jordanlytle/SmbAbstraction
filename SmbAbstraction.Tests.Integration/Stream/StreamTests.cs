using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Xunit;

namespace SmbAbstraction.Tests.Integration.Stream
{
    public abstract class StreamTests
    {
        readonly TestFixture _fixture;
        readonly IFileSystem _fileSystem;
        public StreamTests(TestFixture fixture)
        {
            _fixture = fixture;
            _fileSystem = _fixture.FileSystem;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckStreamLength()
        {
            var tempFileName = $"temp-CheckStreamLength-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var directory = _fileSystem.Path.Combine(_fixture.RootPath, _fixture.Directories.First());
            var tempFilePath = _fileSystem.Path.Combine(_fixture.LocalTempDirectory, tempFileName);

            var byteArray = new byte[100];

            using var credential = new SMBCredential(credentials.Domain, credentials.Username, credentials.Password, directory, _fixture.SMBCredentialProvider);

            if(!_fileSystem.File.Exists(tempFilePath))
            {
                using(var stream = _fileSystem.File.Create(tempFilePath))
                {
                    stream.Write(byteArray, 0, 100);
                }
            }

            var fileInfo = _fileSystem.FileInfo.FromFileName(tempFilePath);
            var fileSize = fileInfo.Length;

            var destinationFilePath = _fileSystem.Path.Combine(directory, tempFileName);
            fileInfo = fileInfo.CopyTo(destinationFilePath);

            Assert.True(fileInfo.Exists);
            
            using (var stream = fileInfo.OpenRead())
            {
                Assert.Equal(stream.Length, fileSize);
            }

            _fileSystem.File.Delete(fileInfo.FullName);
        }
    }
}
