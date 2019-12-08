using System;
using System.Collections.Generic;

namespace SmbAbstraction
{
    public interface ISMBCredentialProvider
    {
        ISMBCredential GetSMBCredential(string path);
        IEnumerable<ISMBCredential> GetSMBCredentials();
        void AddSMBCredential(ISMBCredential credential);
    }
}
