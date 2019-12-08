using System;
using System.Collections.Generic;

namespace SmbAbstraction
{
    public class SMBCredential : ISMBCredential
    {
        public string Domain => _domain;
        public string UserName => _userName;
        public string Password => _password;
        public string Path => _path;

        private string _domain;
        private string _userName;
        private string _password;
        private string _path;
        private List<ISMBCredential> _parentList;

        public SMBCredential(string domain, string userName, string password, string path)
        {
            _domain = domain;
            _userName = userName;
            _password = password;
            _path = path;
        }

        public SMBCredential(string domain, string userName, string password, string path, ISMBCredentialProvider provider)
            : this(domain, userName, password, path)
        {
            provider.AddSMBCredential(this);
        }

        public void Dispose()
        {
            if(_parentList != null)
            {
                _parentList.Remove(this);
            }
        }

        public void SetParentList(List<ISMBCredential> parentList)
        {
            _parentList = parentList;
        }
    }
}
