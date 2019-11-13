﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmbLibraryStd.Client;
using System.Net;
using SmbLibraryStd;
using System.Linq;

namespace System.IO.Abstractions.SMB
{
    public class SMBFile : FileWrapper, IFile, IDisposable
    {
        private ISMBClient _smbClient;
        private ISMBCredentialProvider _credentialProvider;

        public IPAddress ipAddress { get; set; }
        public SMBTransportType transport { get; set; }

        public SMBFile(ISMBClient smbclient, ISMBCredentialProvider credentialProvider) : base(new FileSystem())
        {
            _smbClient = smbclient;
            _credentialProvider = credentialProvider;
            transport = SMBTransportType.DirectTCPTransport;
        }

        public override void AppendAllLines(string path, IEnumerable<string> contents)
        {
            if (!IsSMBPath(path))
            {
                base.AppendAllLines(path, contents);
                return;
            }

            using (Stream s = OpenWrite(path))
            {
                s.Seek(0, SeekOrigin.End);
                using (StreamWriter sw = new StreamWriter(s))
                {
                    sw.Write(contents);
                }
            }
        }

        public override void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                base.AppendAllLines(path, contents, encoding);
                return;
            }

            using (Stream s = OpenWrite(path))
            {
                s.Seek(0, SeekOrigin.End);
                using (StreamWriter sw = new StreamWriter(s, encoding))
                {
                    sw.Write(contents);
                }
            }
        }

        public override Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.AppendAllLinesAsync(path, contents, cancellationToken);
            }

            return new Task(() => AppendAllLines(path, contents), cancellationToken);
        }

        public override Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                base.AppendAllLinesAsync(path, contents, encoding, cancellationToken);
            }

            return new Task(() => AppendAllLines(path, contents, encoding), cancellationToken);
        }

        public override void AppendAllText(string path, string contents)
        {
            if (!IsSMBPath(path))
            {
                base.AppendAllText(path, contents);
                return;
            }

            using (Stream s = OpenWrite(path))
            {
                s.Seek(0, SeekOrigin.End);
                using (StreamWriter sw = new StreamWriter(s))
                {
                    sw.Write(contents);
                }
            }
        }

        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                base.AppendAllText(path, contents, encoding);
                return;
            }

            using (Stream s = OpenWrite(path))
            {
                s.Seek(0, SeekOrigin.End);
                using (StreamWriter sw = new StreamWriter(s, encoding))
                {
                    sw.Write(contents);
                }
            }
        }

        public override Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.AppendAllTextAsync(path, contents, cancellationToken);
            }

            return new Task(() => AppendAllText(path, contents), cancellationToken);
        }

        public override Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.AppendAllTextAsync(path, contents, encoding, cancellationToken);
            }

            return new Task(() => AppendAllText(path, contents, encoding), cancellationToken);
        }

        public override StreamWriter AppendText(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.AppendText(path);
            }

            Stream s = OpenWrite(path);
            s.Seek(0, SeekOrigin.End);
            return new StreamWriter(s);
        }

        public override void Copy(string sourceFileName, string destFileName)
        {
            using (Stream sourceStream = OpenRead(sourceFileName))
            {
                using (Stream destStream = OpenWrite(destFileName))
                {
                    sourceStream.CopyTo(destStream);
                }    
            }
        }

        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if(!overwrite && Exists(destFileName))
            {
                return;
            }

            Copy(sourceFileName, destFileName);
        }

        public override Stream Create(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.Create(path);
            }

            return Open(path, FileMode.Create, FileAccess.ReadWrite);
        }

        public override Stream Create(string path, int bufferSize)
        {
            if (!IsSMBPath(path))
            {
                return base.Create(path, bufferSize);
            }

            return new BufferedStream(Open(path, FileMode.Create, FileAccess.ReadWrite), bufferSize);
        }

        public override Stream Create(string path, int bufferSize, FileOptions options)
        {
            if (!IsSMBPath(path))
            {
                return base.Create(path, bufferSize, options);
            }

            return Create(path, bufferSize, options);
        }

        public override StreamWriter CreateText(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.CreateText(path);
            }

            return new StreamWriter(OpenWrite(path));
        }

        public override void Delete(string path)
        {
            if (!IsSMBPath(path))
            {
                base.Delete(path);
                return;
            }

            Uri uri = new Uri(path);
            var hostEntry = Dns.GetHostEntry(uri.Host);
            ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (_smbClient.Connect(ipAddress, transport))
            {
                var credential = _credentialProvider.GetSMBCredential(path);
                status = _smbClient.Login(credential.GetDomain(), credential.GetUserName(), credential.GetPassword());

                var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
                var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1);
                var directoryPath = Path.GetDirectoryName(newPath);

                ISMBFileStore fileStore = _smbClient.TreeConnect(shareName, out status);

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, AccessMask.DELETE, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_DELETE_ON_CLOSE, null);
                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new IOException($"Unable to connect to smbShare. Status = {status}");
                }

                // There should be a seperate option to delete, but it doesn't seem to exsist in the library we are using, so this should work for now. Really hacky though.
                fileStore.CloseFile(handle);
            }
        }

        public override bool Exists(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.Exists(path);
            }

            Uri uri = new Uri(path);
            var hostEntry = Dns.GetHostEntry(uri.Host);
            ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            if (_smbClient.Connect(ipAddress, transport))
            {
                var credential = _credentialProvider.GetSMBCredential(path);
                status = _smbClient.Login(credential.GetDomain(), credential.GetUserName(), credential.GetPassword());

                var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
                var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1);
                var directoryPath = Path.GetDirectoryName(newPath);

                ISMBFileStore fileStore = _smbClient.TreeConnect(shareName, out status);

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, directoryPath, AccessMask.GENERIC_READ, 0, ShareAccess.Read,
                    CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new IOException($"Unable to connect to smbShare. Status = {status}");
                }

                fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> queryDirectoryFileInformation, handle, string.IsNullOrEmpty(directoryPath) ? "*" : directoryPath, FileInformationClass.FileDirectoryInformation);

                foreach(var file in queryDirectoryFileInformation)
                {
                    if(file.FileInformationClass == FileInformationClass.FileDirectoryInformation)
                    {
                        FileDirectoryInformation fileDirectoryInformation = (FileDirectoryInformation)file;
                        if(fileDirectoryInformation.FileName == Path.GetFileName(newPath))
                        {
                            fileStore.CloseFile(handle);
                            return true;
                        }
                    }
                }

                fileStore.CloseFile(handle);
            }

            return false;
        }

        public override FileSecurity GetAccessControl(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetAccessControl(path);
            }
            throw new NotImplementedException();
        }

        public override FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            if (!IsSMBPath(path))
            {
                return base.GetAccessControl(path, includeSections);
            }
            throw new NotImplementedException();
        }

        public override FileAttributes GetAttributes(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetAttributes(path);
            }
            throw new NotImplementedException();
        }

        public override DateTime GetCreationTime(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetCreationTime(path);
            }
            throw new NotImplementedException();
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetCreationTimeUtc(path);
            }
            throw new NotImplementedException();
        }

        public override DateTime GetLastAccessTime(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetLastAccessTime(path);
            }
            throw new NotImplementedException();
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetLastAccessTimeUtc(path);
            }
            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTime(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetLastWriteTime(path);
            }
            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.GetLastWriteTimeUtc(path);
            }
            throw new NotImplementedException();
        }

        public override void Move(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }

        public override Stream Open(string path, FileMode mode)
        {
            if (!IsSMBPath(path))
            {
                return base.Open(path, mode);
            }
            return Open(path, mode, FileAccess.ReadWrite);
        }

        public override Stream Open(string path, FileMode mode, FileAccess access)
        {
            if (!IsSMBPath(path))
            {
                return base.Open(path, mode, access);
            }
            return Open(path, mode, access, FileShare.Read);
        }

        public override Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return Open(path, mode, access, share, FileOptions.None);
        }

        private Stream Open(string path, FileMode mode, FileAccess access, FileShare share, FileOptions fileOptions)
        {
            if (!IsSMBPath(path))
            {
                return base.Open(path, mode, access, share);
            }

            Uri uri = new Uri(path);
            var hostEntry = Dns.GetHostEntry(uri.Host);
            ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == Net.Sockets.AddressFamily.InterNetwork);

            NTStatus status = NTStatus.STATUS_SUCCESS;

            AccessMask accessMask = AccessMask.MAXIMUM_ALLOWED;
            ShareAccess shareAccess = ShareAccess.None;
            CreateDisposition disposition = CreateDisposition.FILE_OPEN;
            CreateOptions createOptions;

            switch (fileOptions)
            {
                case FileOptions.DeleteOnClose:
                    createOptions = CreateOptions.FILE_DELETE_ON_CLOSE;
                    break;
                case FileOptions.RandomAccess:
                    createOptions = CreateOptions.FILE_RANDOM_ACCESS;
                    break;
                case FileOptions.SequentialScan:
                    createOptions = CreateOptions.FILE_SEQUENTIAL_ONLY;
                    break;
                case FileOptions.WriteThrough:
                    createOptions = CreateOptions.FILE_WRITE_THROUGH;
                    break;
                case FileOptions.None:
                case FileOptions.Encrypted:     // These two are not suported unless I am missing something 
                case FileOptions.Asynchronous:  //
                default:
                    createOptions = CreateOptions.FILE_NON_DIRECTORY_FILE;
                    break;
            }

            switch(access)
            {
                case FileAccess.Read:
                    accessMask = AccessMask.GENERIC_READ;
                    shareAccess = ShareAccess.Read;
                    break;
                case FileAccess.Write:
                    accessMask = AccessMask.GENERIC_WRITE;
                    shareAccess = ShareAccess.Write;
                    break;
                case FileAccess.ReadWrite:
                    accessMask = AccessMask.GENERIC_ALL;
                    shareAccess = ShareAccess.Write;
                    break;
            }

            if (_smbClient.Connect(ipAddress, transport))
            {
                var credential = _credentialProvider.GetSMBCredential(path);
                if(credential == null)
                {
                    throw new Exception($"Unable to find credential for path: {path}");
                }
                status = _smbClient.Login(credential.GetDomain(), credential.GetUserName(), credential.GetPassword());

                var shareName = uri.Segments[1].Replace(Path.DirectorySeparatorChar.ToString(), "");
                var newPath = uri.AbsolutePath.Replace(uri.Segments[1], "").Remove(0, 1);

                ISMBFileStore fileStore = _smbClient.TreeConnect(shareName, out status);


                switch (mode)
                {
                    case FileMode.Create:
                        disposition = CreateDisposition.FILE_CREATE;
                        break;
                    case FileMode.CreateNew:
                        disposition = CreateDisposition.FILE_OVERWRITE;
                        break;
                    case FileMode.Open:
                        disposition = CreateDisposition.FILE_OPEN;
                        break;
                    case FileMode.OpenOrCreate:
                        disposition = Exists(path) ? CreateDisposition.FILE_OPEN : CreateDisposition.FILE_CREATE;
                        break;
                }

                status = fileStore.CreateFile(out object handle, out FileStatus fileStatus, newPath, accessMask, 0, shareAccess,
                    disposition, createOptions, null);
                if(status != NTStatus.STATUS_SUCCESS)
                {
                    throw new IOException($"Unable to connect to smbShare. Status = {status}");
                }

                Stream s = new SMBStream(fileStore, handle);

                if(mode == FileMode.Append)
                {
                    s.Seek(0, SeekOrigin.End);
                }

                return s;
            }
            else
            {
                throw new IOException($"Unable to connect to smbShare. Status = {status}");
            }
        }

        public override Stream OpenRead(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.OpenRead(path);
            }

            return Open(path, FileMode.Open, FileAccess.Read);
        }

        public override StreamReader OpenText(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.OpenText(path);
            }

            return new StreamReader(OpenRead(path));
        }

        public override Stream OpenWrite(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.OpenWrite(path);
            }

            return Open(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public override byte[] ReadAllBytes(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllBytes(path);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream s = OpenRead(path))
                {
                    s.CopyTo(ms);
                }
                return ms.ToArray();
            }

        }

        public override Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllBytesAsync(path, cancellationToken);
            }

            return new Task<byte[]>(() => ReadAllBytes(path), cancellationToken);
        }

        public override string[] ReadAllLines(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllLines(path);
            }

            return ReadLines(path).ToArray();
        }

        public override string[] ReadAllLines(string path, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllLines(path, encoding);
            }

            return ReadLines(path, encoding).ToArray();
        }

        public override Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllLinesAsync(path, cancellationToken);
            }

            return new Task<string[]>(() => ReadAllLines(path), cancellationToken);
        }

        public override Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllLinesAsync(path, encoding, cancellationToken);
            }

            return new Task<string[]>(() => ReadAllLines(path, encoding), cancellationToken);
        }

        public override string ReadAllText(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllText(path);
            }

            using (StreamReader sr = new StreamReader(OpenRead(path)))
            {
                return sr.ReadToEnd();
            }
        }

        public override string ReadAllText(string path, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllText(path, encoding);
            }

            using (StreamReader sr = new StreamReader(OpenRead(path), encoding))
            {
                return sr.ReadToEnd();
            }
        }

        public override Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllTextAsync(path, cancellationToken);
            }

            return new Task<string>(() => ReadAllText(path), cancellationToken);
        }

        public override Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.ReadAllTextAsync(path, encoding, cancellationToken);
            }

            return new Task<string>(() => ReadAllText(path, encoding), cancellationToken);
        }

        public override IEnumerable<string> ReadLines(string path)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadLines(path);
            }

            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(OpenRead(path)))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        public override IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                return base.ReadLines(path, encoding);
            }

            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(OpenRead(path), encoding))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        public override void SetAccessControl(string path, FileSecurity fileSecurity)
        {
            if (!IsSMBPath(path))
            {
                base.SetAccessControl(path, fileSecurity);
                return;
            }
            throw new NotSupportedException();
        }

        public override void SetAttributes(string path, FileAttributes fileAttributes)
        {
            if (!IsSMBPath(path))
            {
                base.SetAttributes(path, fileAttributes);
                return;
            }
            throw new NotSupportedException();
        }

        public override void SetCreationTime(string path, DateTime creationTime)
        {
            if (!IsSMBPath(path))
            {
                base.SetCreationTime(path, creationTime);
                return;
            }

            throw new NotSupportedException();
        }

        public override void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            if (!IsSMBPath(path))
            {
                base.SetCreationTimeUtc(path, creationTimeUtc);
                return;
            }

            throw new NotSupportedException();
        }

        public override void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            if (!IsSMBPath(path))
            {
                base.SetLastAccessTime(path, lastAccessTime);
                return;
            }

            throw new NotSupportedException();
        }

        public override void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            if (!IsSMBPath(path))
            {
                base.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
                return;
            }

            throw new NotSupportedException();
        }

        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            if (!IsSMBPath(path))
            {
                base.SetLastWriteTime(path, lastWriteTime);
                return;
            }

            throw new NotSupportedException();
        }

        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            if (!IsSMBPath(path))
            {
                base.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
                return;
            }

            throw new NotSupportedException();
        }

        public override void WriteAllBytes(string path, byte[] bytes)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllBytes(path, bytes);
                return;
            }

            using (StreamWriter sr = new StreamWriter(OpenWrite(path)))
            {
                sr.Write(bytes);
            }
        }

        public override Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllBytesAsync(path, bytes, cancellationToken);
            }

            return new Task(() => WriteAllBytes(path, bytes), cancellationToken);
        }

        public override void WriteAllLines(string path, IEnumerable<string> contents)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllLines(path, contents);
                return;
            }

            WriteAllLines(path, contents.ToArray());
        }

        public override void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllLines(path, contents, encoding);
                return;
            }

            WriteAllLines(path, contents.ToArray(), encoding);
        }

        public override void WriteAllLines(string path, string[] contents)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllLines(path, contents);
                return;
            }

            using (StreamWriter sr = new StreamWriter(OpenWrite(path)))
            {
                sr.Write(contents);
            }
        }

        public override void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllLines(path, contents, encoding);
                return;
            }

            using (StreamWriter sr = new StreamWriter(OpenWrite(path), encoding))
            {
                sr.Write(contents);
            }
        }

        public override Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllLinesAsync(path, contents, cancellationToken);
            }

            return new Task(() => WriteAllLines(path, contents), cancellationToken);
        }

        public override Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
            }

            return new Task(() => WriteAllLines(path, contents, encoding), cancellationToken);
        }

        public override Task WriteAllLinesAsync(string path, string[] contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllLinesAsync(path, contents, cancellationToken);
            }

            return new Task(() => WriteAllLines(path, contents), cancellationToken);
        }

        public override Task WriteAllLinesAsync(string path, string[] contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
            }

            return new Task(() => WriteAllLines(path, contents, encoding), cancellationToken);
        }

        public override void WriteAllText(string path, string contents)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllText(path, contents);
                return;
            }

            using (StreamWriter sw = new StreamWriter(OpenWrite(path)))
            {
                sw.Write(contents);
            }
        }

        public override void WriteAllText(string path, string contents, Encoding encoding)
        {
            if (!IsSMBPath(path))
            {
                base.WriteAllText(path, contents, encoding);
            }

            using (StreamWriter sw = new StreamWriter(OpenWrite(path), encoding))
            {
                sw.Write(contents);
            }
        }

        public override Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllTextAsync(path, contents, cancellationToken);
            }

            return new Task(() => WriteAllText(path, contents), cancellationToken);
        }

        public override Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsSMBPath(path))
            {
                return base.WriteAllTextAsync(path, contents, encoding, cancellationToken);
            }

            return new Task(() => WriteAllText(path, contents, encoding), cancellationToken);
        }

        public void Dispose()
        {
            _smbClient.Logoff();
            _smbClient.Disconnect();
        }

        private bool IsSMBPath(string path)
        {
            return new Uri(path).IsUnc || path.StartsWith("smb://");
        }
    }
}