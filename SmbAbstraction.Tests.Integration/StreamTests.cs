using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SmbAbstraction.Tests.Integration
{
    public class StreamTests : TestBase
    {
        public StreamTests() : base()
        {
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckStreamLength()
        {
            var tempFileName = $"temp-CheckStreamLength-{DateTime.Now.ToFileTimeUtc()}.txt";
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = Path.Combine(testRootUncPath, testShare.Directories.First());
            var tempFilePath = Path.Combine(TestSettings.LocalTempFolder, tempFileName);

            var byteArray = new byte[100];

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            if(!FileSystem.File.Exists(tempFilePath))
            {
                using(var stream = FileSystem.File.Create(tempFilePath))
                {
                    stream.Write(byteArray, 0, 100);
                }
            }

            var fileInfo = FileSystem.FileInfo.FromFileName(tempFilePath);
            var fileSize = fileInfo.Length;
            
            var uncFileInfo = fileInfo.CopyTo(Path.Combine(uncDirectory, tempFileName));
            Assert.True(uncFileInfo.Exists);

            using(var stream = uncFileInfo.OpenRead())
            {
                Assert.Equal(stream.Length, fileSize);
            }
        }
    }
}
