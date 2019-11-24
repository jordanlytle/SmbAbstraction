using System;
using System.Collections.Generic;

namespace System.IO.Abstractions.SMB
{
    public interface ISMBCredentialProvider
    {
        ISMBCredential GetSMBCredential(string path);
        IEnumerable<ISMBCredential> GetSMBCredentials();
        void AddSMBCredential(ISMBCredential credential);
    }
}
