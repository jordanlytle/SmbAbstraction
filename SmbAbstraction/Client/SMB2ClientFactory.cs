using System;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMB2ClientFactory : ISMBClientFactory
    {
        public ISMBClient CreateClient(uint maxBufferSize)
        {
            var client = new SMB2Client();
            return client;
        }
    }
}
