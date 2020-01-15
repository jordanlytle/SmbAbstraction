using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SmbAbstraction.Tests.Integration
{
    public class FileInfoTests : TestBase
    {
        public FileInfoTests() : base()
        {
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CopyFromLocalDirectoryToUncDirectory()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = Path.Combine(testRootUncPath, testShare.Directories.First());
            var tempFilePath = Path.Combine(TestSettings.LocalTempFolder, tempFileName);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            if(!FileSystem.File.Exists(tempFilePath))
            {
                using(var streamWriter = new StreamWriter(FileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            var fileInfo = FileSystem.FileInfo.FromFileName(tempFilePath);
            
            var uncFileInfo = fileInfo.CopyTo(Path.Combine(uncDirectory, tempFileName));
            Assert.True(uncFileInfo.Exists);
        }
    }
}
