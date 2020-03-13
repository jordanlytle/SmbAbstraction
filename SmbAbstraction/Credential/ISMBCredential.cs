using System;

namespace SmbAbstraction
{
    public interface ISMBCredential : IDisposable
    {
        string Domain { get; }
        string UserName { get; }
        string Password { get; }
        string Path { get; }
        string Host { get; }
        string ShareName { get; }
    }
}
