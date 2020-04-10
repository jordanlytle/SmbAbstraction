using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Xunit;

namespace SmbAbstraction.Tests.Integration.File
{
    public abstract class FileTests
    {
        readonly TestFixture _fixture;
        private IFileSystem _fileSystem;

        public FileTests(TestFixture fixture)
        {
            _fixture = fixture;
            _fileSystem = _fixture.FileSystem;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckDeleteCompletes()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var credentials = _fixture.ShareCredentials;
            var share = _fixture.Shares.First();
            var rootPath = share.GetRootPath(_fixture.PathType);
            var directory = _fileSystem.Path.Combine(rootPath, share.Directories.First());
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
    }
}
