using System;
using System.Collections.Generic;
using System.Text;

namespace SmbAbstraction
{
    public class SMBException : Exception
    {
        public SMBException(string message) : base(message)
        {
        }

        public SMBException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}