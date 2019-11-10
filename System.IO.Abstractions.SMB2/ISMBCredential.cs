using System;
namespace System.IO.Abstractions.SMB
{
    public interface ISMBCredential
    {
        string GetDomain();
        string GetUserName();
        string GetPassword();
        Guid GetUID();
    }
}
