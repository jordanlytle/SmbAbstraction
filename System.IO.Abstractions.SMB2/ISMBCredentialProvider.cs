using System;
namespace System.IO.Abstractions.SMB
{
    public interface ISMBCredentialProvider
    {
        ISMBCredential GetSMBCredential(string path);
        void AddSMBCredential(ISMBCredential credential);
    }
}
