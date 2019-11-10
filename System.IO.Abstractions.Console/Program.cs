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
            string testDomain = "";
            string testPath1 = "<test path one>";
            string testPath2 = "<test path two>";

            ISMBCredential credential = new SMBCredential(testDomain, testUsername, testPassword);
            ISMBCredentialProvider credentialProvider = new SMBCredentialProvider();

            IFileSystem fileSystem = new SMBFileSystem(new SMB2Client(), credentialProvider);

            credentialProvider.SetSMBCredential(testPath1, credential);
            credentialProvider.SetSMBCredential(testPath2, credential);

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
        }
    }
}