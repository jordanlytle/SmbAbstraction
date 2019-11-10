using System;
namespace System.IO.Abstractions.SMB
{
    public interface ISMBCredentialProvider
    {
        ISMBCredential GetSMBCredential(string path);
        void SetSMBCredential(string path, ISMBCredential credential);
        void RemoveSMBCredential(string path, ISMBCredential credential);
    }
}
