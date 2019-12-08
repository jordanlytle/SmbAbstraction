using System;
using SmbLibraryStd.Client;

namespace SmbAbstraction
{
    public interface ISMBClientFactory
    {
        ISMBClient CreateClient();
    }
}
