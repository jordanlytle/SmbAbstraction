using System;
using System.Collections.Generic;

namespace System.IO.Abstractions.SMB
{
    public interface ISMBCredential : IDisposable
    {
        string GetDomain();
        string GetUserName();
        string GetPassword();
        string GetPath();
        void SetParentList(List<ISMBCredential> parentList);
    }
}
