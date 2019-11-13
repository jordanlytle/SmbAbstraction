using System;
using System.IO.Abstractions.SMB;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string testUsername = "<test user>";
            string testPassword = "<test password>";
            string testDomain = "<test domain name>";
            string testPath1 = "<test file>";
            string testPath2 = "<second test file>";
            string testPath3 = "<test share root>";
            string testPath4 = "<folder in test share>";

            ISMBCredentialProvider credentialProvider = new SMBCredentialProvider();

            IFileSystem fileSystem = new SMBFileSystem(new SMB2Client(), credentialProvider);

            using ISMBCredential credential1 = new SMBCredential(testDomain, testUsername, testPassword, testPath1, credentialProvider);
            using ISMBCredential credential2 = new SMBCredential(testDomain, testUsername, testPassword, testPath2, credentialProvider);

            using (StreamWriter sr = new StreamWriter(fileSystem.File.OpenWrite(testPath1)))
            {
                sr.WriteLine($"This was written over the network at {DateTime.Now}");
            }

            using (StreamWriter sr = fileSystem.File.AppendText(testPath1))
            {
                sr.WriteLine($"This was appended over the network at {DateTime.Now}");
            }

            fileSystem.File.AppendAllText(testPath1, $"\nAll this text was apppended at once at {DateTime.Now}");

            using (StreamReader sr = new StreamReader(fileSystem.File.OpenRead(testPath1)))
            {
                System.Console.WriteLine(sr.ReadToEnd());
            }

            System.Console.WriteLine("Copying...");

            fileSystem.File.Copy(testPath1, testPath2);

            using (StreamReader sr = new StreamReader(fileSystem.File.OpenRead(testPath2)))
            {
                System.Console.WriteLine(sr.ReadToEnd());
            }

            System.Console.WriteLine("Attempting to create and then read a file with the same stream.");


            var tempFileName = $"{testPath1}_{DateTime.Now.Millisecond}.txt";
            using (var tempCred = new SMBCredential(testDomain, testUsername, testPassword, tempFileName, credentialProvider))
            {
                using (Stream s = fileSystem.File.Create(tempFileName, 8))
                {
                    StreamWriter sw = new StreamWriter(s);
                    sw.WriteLine($"This was written to a temp file at {DateTime.Now}");
                    sw.Flush();

                    s.Seek(0, SeekOrigin.Begin);
                    StreamReader sr = new StreamReader(s);
                    System.Console.Write(sr.ReadToEnd());

                    sw.Close();
                    sr.Close();
                }
            }

            System.Console.WriteLine("Enumerating share files");
            using var credential3 = new SMBCredential(testDomain, testUsername, testPassword, testPath3, credentialProvider);

            foreach (var file in fileSystem.Directory.EnumerateFiles(testPath3, "*"))
            {
                System.Console.WriteLine(file);
            }

            System.Console.WriteLine("Enumerating directory files");
            using var credential4 = new SMBCredential(testDomain, testUsername, testPassword, testPath4, credentialProvider);
            foreach (var file in fileSystem.Directory.EnumerateFiles(testPath4))
            {
                System.Console.WriteLine(file);
            }

            System.Console.WriteLine("Deleting file.");
            using var credential5 = new SMBCredential(testDomain, testUsername, testPassword, tempFileName);
            credentialProvider.AddSMBCredential(credential5);
            fileSystem.File.Delete(tempFileName);
        }
    }
}