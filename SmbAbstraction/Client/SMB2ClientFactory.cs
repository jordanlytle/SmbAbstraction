using System;
using SmbLibraryStd.Client;

namespace SmbAbstraction
{
    public class SMB2ClientFactory : ISMBClientFactory
    {
        public ISMBClient CreateClient()
        {
            return new SMB2Client();
        }
    }
}
