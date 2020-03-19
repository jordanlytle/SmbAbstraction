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
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());
            var tempFilePath = FileSystem.Path.Combine(LocalTempDirectory, tempFileName);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            if (!FileSystem.File.Exists(tempFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(tempFilePath)))
                {
                    streamWriter.WriteLine("Test");
                }
            }

            var fileInfo = FileSystem.FileInfo.FromFileName(tempFilePath);

            var uncFileInfo = fileInfo.CopyTo(FileSystem.Path.Combine(uncDirectory, tempFileName));
            Assert.True(uncFileInfo.Exists);

            FileSystem.File.Delete(tempFilePath);
            FileSystem.File.Delete(uncFileInfo.FullName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFileSize()
        {
            var tempFileName = $"temp-{DateTime.Now.ToFileTimeUtc()}.txt";
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());
            var tempFilePath = FileSystem.Path.Combine(LocalTempDirectory, tempFileName);

            var byteArray = new byte[100];

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            if (!FileSystem.File.Exists(tempFilePath))
            {
                using (var stream = FileSystem.File.Create(tempFilePath))
                {
                    stream.Write(byteArray, 0, 100);
                }
            }

            var fileInfo = FileSystem.FileInfo.FromFileName(tempFilePath);
            var fileSize = fileInfo.Length;

            var destinationFilePath = FileSystem.Path.Combine(uncDirectory, tempFileName);
            var uncFileInfo = fileInfo.CopyTo(destinationFilePath);
            
            Assert.True(uncFileInfo.Exists);
            Assert.Equal(fileSize, uncFileInfo.Length);

            FileSystem.File.Delete(uncFileInfo.FullName);
            FileSystem.File.Delete(tempFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFileExists()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());
            var filePath = FileSystem.Path.Combine(testRootUncPath, testShare.Files.First());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            var exists = FileSystem.FileInfo.FromFileName(filePath).Exists;

            Assert.True(exists);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckFileExtensionMatches()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());
            var filePath = FileSystem.Path.Combine(testRootUncPath, testShare.Files.First());
            var fileExtension = FileSystem.Path.GetExtension(filePath);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            var extenstion = FileSystem.FileInfo.FromFileName(filePath).Extension;

            Assert.Equal(fileExtension, extenstion);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckUncFullNameMatches()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());
            var filePath = FileSystem.Path.Combine(testRootUncPath, testShare.Files.First());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            var fullName = FileSystem.FileInfo.FromFileName(filePath).FullName;

            Assert.Equal(filePath, fullName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckSmbFullNameMatches()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootSmbUri = testShare.RootSmbUri;
            //var smbUriDirectory = Path.Combine(testRootSmbUri, testShare.Directories.First());
            //var filePath = Path.Combine(testRootSmbUri, testShare.Files.First());
            var smbUriDirectory = $"{testRootSmbUri}/";
            var filePath = $"{testRootSmbUri}/{testShare.Files.First()}";

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, smbUriDirectory, SMBCredentialProvider);

            var fullName = FileSystem.FileInfo.FromFileName(filePath).FullName;

            Assert.Equal(filePath, fullName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckReplaceForUncPathWithBackup()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            var originalFileTime = DateTime.Now.ToFileTimeUtc();
            var originalFilePath = FileSystem.Path.Combine(uncDirectory, $"replace-file-{originalFileTime}.txt");
            var originalFileBackupPath = FileSystem.Path.Combine(uncDirectory, $"replace-file-{originalFileTime}.bak");

           if (!FileSystem.File.Exists(originalFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(originalFilePath)))
                {
                    streamWriter.WriteLine($"{originalFileTime}");
                }
            }

            var newFileTime = DateTime.Now.ToFileTimeUtc();
            var newFilePath = FileSystem.Path.Combine(testRootUncPath, $"replace-file-{newFileTime}.txt");

            if (!FileSystem.File.Exists(newFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(newFilePath)))
                {
                    streamWriter.WriteLine($"{newFileTime}");
                }
            }

            var newFileInfo = FileSystem.FileInfo.FromFileName(newFilePath);

            newFileInfo = newFileInfo.Replace(originalFilePath, originalFileBackupPath);

            Assert.Equal(originalFilePath, newFileInfo.FullName);
            Assert.False(FileSystem.File.Exists(newFilePath));
            Assert.True(FileSystem.File.Exists(originalFileBackupPath));

            using (var streamReader = new StreamReader(FileSystem.File.OpenRead(newFileInfo.FullName)))
            {
                var line = streamReader.ReadLine();

                Assert.Equal(newFileTime.ToString(), line);
            }

            FileSystem.File.Delete(originalFilePath);
            FileSystem.File.Delete(originalFileBackupPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckReplaceForUncPathWithoutBackup()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            var originalFileTime = DateTime.Now.ToFileTimeUtc();
            var originalFilePath = FileSystem.Path.Combine(uncDirectory, $"replace-file-{originalFileTime}.txt");

            if (!FileSystem.File.Exists(originalFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(originalFilePath)))
                {
                    streamWriter.WriteLine($"{originalFileTime}");
                }
            }

            var newFileTime = DateTime.Now.ToFileTimeUtc();
            var newFilePath = FileSystem.Path.Combine(testRootUncPath, $"replace-file-{newFileTime}.txt");

            if (!FileSystem.File.Exists(newFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(newFilePath)))
                {
                    streamWriter.WriteLine($"{newFileTime}");
                }
            }

            var newFileInfo = FileSystem.FileInfo.FromFileName(newFilePath);

            newFileInfo = newFileInfo.Replace(originalFilePath, "");

            Assert.Equal(originalFilePath, newFileInfo.FullName);
            Assert.False(FileSystem.File.Exists(newFilePath));

            using (var streamReader = new StreamReader(FileSystem.File.OpenRead(newFileInfo.FullName)))
            {
                var line = streamReader.ReadLine();

                Assert.Equal(newFileTime.ToString(), line);
            }

            FileSystem.File.Delete(originalFilePath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckReplaceForSmbUriWithBackup()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootSmbUri = testShare.RootSmbUri;
            var smbDirectory = FileSystem.Path.Combine(testRootSmbUri, testShare.Directories.First());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, smbDirectory, SMBCredentialProvider);

            var originalFileTime = DateTime.Now.ToFileTimeUtc();
            var originalFilePath = FileSystem.Path.Combine(smbDirectory, $"replace-file-{originalFileTime}.txt");
            var originalFileBackupPath = FileSystem.Path.Combine(smbDirectory, $"replace-file-{originalFileTime}.bak");

            if (!FileSystem.File.Exists(originalFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(originalFilePath)))
                {
                    streamWriter.WriteLine($"{originalFileTime}");
                }
            }

            var newFileTime = DateTime.Now.ToFileTimeUtc();
            var newFilePath = FileSystem.Path.Combine(testRootSmbUri, $"replace-file-{newFileTime}.txt");

            if (!FileSystem.File.Exists(newFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(newFilePath)))
                {
                    streamWriter.WriteLine($"{newFileTime}");
                }
            }

            var newFileInfo = FileSystem.FileInfo.FromFileName(newFilePath);

            newFileInfo = newFileInfo.Replace(originalFilePath, originalFileBackupPath);

            Assert.Equal(originalFilePath, newFileInfo.FullName);
            Assert.False(FileSystem.File.Exists(newFilePath));
            Assert.True(FileSystem.File.Exists(originalFileBackupPath));

            using (var streamReader = new StreamReader(FileSystem.File.OpenRead(newFileInfo.FullName)))
            {
                var line = streamReader.ReadLine();

                Assert.Equal(newFileTime.ToString(), line);
            }

            FileSystem.File.Delete(originalFilePath);
            FileSystem.File.Delete(originalFileBackupPath);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckReplaceForSmbUriWithoutBackup()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootSmbUri = testShare.RootSmbUri;
            var smbDirectory = FileSystem.Path.Combine(testRootSmbUri, testShare.Directories.First());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, smbDirectory, SMBCredentialProvider);

            var originalFileTime = DateTime.Now.ToFileTimeUtc();
            var originalFilePath = FileSystem.Path.Combine(smbDirectory, $"replace-file-{originalFileTime}.txt");

            if (!FileSystem.File.Exists(originalFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(originalFilePath)))
                {
                    streamWriter.WriteLine($"{originalFileTime}");
                }
            }

            var newFileTime = DateTime.Now.ToFileTimeUtc();
            var newFilePath = FileSystem.Path.Combine(testRootSmbUri, $"replace-file-{newFileTime}.txt");

            if (!FileSystem.File.Exists(newFilePath))
            {
                using (var streamWriter = new StreamWriter(FileSystem.File.Create(newFilePath)))
                {
                    streamWriter.WriteLine($"{newFileTime}");
                }
            }

            var newFileInfo = FileSystem.FileInfo.FromFileName(newFilePath);

            newFileInfo = newFileInfo.Replace(originalFilePath, "");

            Assert.Equal(originalFilePath, newFileInfo.FullName);
            Assert.False(FileSystem.File.Exists(newFilePath));

            using (var streamReader = new StreamReader(FileSystem.File.OpenRead(newFileInfo.FullName)))
            {
                var line = streamReader.ReadLine();

                Assert.Equal(newFileTime.ToString(), line);
            }

            FileSystem.File.Delete(originalFilePath);
        }
    }
}
