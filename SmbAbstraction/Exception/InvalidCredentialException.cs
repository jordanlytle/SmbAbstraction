using System;

namespace SmbAbstraction
{
    [Serializable]
    public class InvalidCredentialException : Exception
    {
        public InvalidCredentialException() : base()
        {
        }

        public InvalidCredentialException(string message) : base(message)
        {
        }        
    }
}
