using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBStream : Stream
    {
        private readonly ISMBFileStore _fileStore;
        private readonly object _fileHandle;
        private readonly SMBConnection _connection;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        private long _length { get; set; }
        private long _position { get; set; }
        public override long Length => _length;
        public override long Position { get { return _position; } set { _position = value; } }


        public SMBStream(ISMBFileStore fileStore, object fileHandle, SMBConnection connection)
        {
            _fileStore = fileStore;
            _fileHandle = fileHandle;
            _connection = connection;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var status = _fileStore.ReadFile(out byte[] data, _fileHandle, _position, count);
            switch (status)
            {
                case SmbLibraryStd.NTStatus.STATUS_SUCCESS:
                    for (int i = offset, i2 = 0; i2 < data.Length; i++, i2++)
                    {
                        buffer[i] = data[i2];
                    }
                    _position += data.Length;
                    return data.Length;
                case SmbLibraryStd.NTStatus.STATUS_END_OF_FILE:
                    return 0;
                default:
                    throw new IOException($"Unable to read file; Status: {status}");
            }

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = 0;
                    break;
                case SeekOrigin.Current:
                    break;
                case SeekOrigin.End:
                    var status = _fileStore.GetFileInformation(out SmbLibraryStd.FileInformation result, _fileHandle, SmbLibraryStd.FileInformationClass.FileStreamInformation);
                    if (status == SmbLibraryStd.NTStatus.STATUS_SUCCESS)
                    {
                        SmbLibraryStd.FileStreamInformation fileStreamInformation = (SmbLibraryStd.FileStreamInformation)result;
                        _position += fileStreamInformation.Entries[0].StreamSize;
                        return _position;
                    }
                    else
                    {
                        throw new IOException($"Unable to seek file. Unable to read file information. Status = {status}");
                    }
            }
            _position += offset;
            return _position;
        }

        public override void SetLength(long value)
        {
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];

            for (int i = offset, i2 = 0; i < count; i++, i2++) 
            {
                data[i2] = buffer[i];
            }

            var status = _fileStore.WriteFile(out int bytesWritten, _fileHandle, _position, data);
            if(status != SmbLibraryStd.NTStatus.STATUS_SUCCESS)
            {
                throw new IOException($"Unable to write to share. Status: {status}");
            }

            _position += bytesWritten;
        }

        public override void Close()
        {
            _fileStore.CloseFile(_fileHandle);
            _connection.Dispose();
            base.Close();
        }
    }
}
