using System;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public interface ISMBClientFactory
    {
        ISMBClient CreateClient();
    }
}
