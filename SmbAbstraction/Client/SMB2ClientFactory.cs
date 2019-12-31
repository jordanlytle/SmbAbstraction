using System;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMB2ClientFactory : ISMBClientFactory
    {
        public ISMBClient CreateClient(uint maxBufferSize)
        {
            var client = new SMB2Client
            {
                ClientMaxReadSize = maxBufferSize,
                ClientMaxWriteSize = maxBufferSize,
                ClientMaxTransactSize = maxBufferSize
            };
            return client;
        }
    }
}
