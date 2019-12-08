using System;
using System.Collections.Generic;

namespace SmbAbstraction
{
    public interface ISMBCredential : IDisposable
    {
        string Domain { get; }
        string UserName { get; }
        string Password { get; }
        string Path { get; }
        void SetParentList(List<ISMBCredential> parentList);
    }
}
