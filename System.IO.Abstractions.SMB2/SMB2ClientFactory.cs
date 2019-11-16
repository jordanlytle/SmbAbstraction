using System;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMB2ClientFactory : ISMBClientFactory
    {
        public ISMBClient CreateClient()
        {
            return new SMB2Client();
        }
    }
}
