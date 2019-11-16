using System;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public interface ISMBClientFactory
    {
        ISMBClient CreateClient();
    }
}
