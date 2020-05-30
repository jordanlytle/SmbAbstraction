using System;
using System.Collections.Generic;
using System.Text;

namespace SmbAbstraction
{
    [Serializable]
    public class SMBException : Exception
    {
        public SMBException() : base()
        {

        }

        public SMBException(string message) : base(message)
        {
        }

        public SMBException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}