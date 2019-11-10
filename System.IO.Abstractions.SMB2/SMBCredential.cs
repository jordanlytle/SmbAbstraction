using System;
namespace System.IO.Abstractions.SMB
{
    public class SMBCredential : ISMBCredential
    {
        private string _domain;
        private string _userName;
        private string _password;
        private Guid _uid;

        public SMBCredential(string domain, string userName, string password)
        {
            _domain = domain;
            _userName = userName;
            _password = password;
            _uid = Guid.NewGuid();
        }

        public string GetDomain()
        {
            return _domain;
        }

        public string GetPassword()
        {
            return _password;
        }

        public Guid GetUID()
        {
            return _uid;
        }

        public string GetUserName()
        {
            return _userName;
        }
    }
}
