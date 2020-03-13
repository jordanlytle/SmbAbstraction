using System;

namespace SmbAbstraction
{
    public class InvalidCredentialException : Exception
    {
        public InvalidCredentialException(string message) : base(message)
        {
        }        
    }
}
