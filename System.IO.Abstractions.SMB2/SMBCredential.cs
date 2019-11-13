using System;
using System.Collections.Generic;

namespace System.IO.Abstractions.SMB
{
    public class SMBCredential : ISMBCredential
    {
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

        public string GetDomain()
        {
            return _domain;
        }

        public string GetPassword()
        {
            return _password;
        }

        public string GetUserName()
        {
            return _userName;
        }

        public string GetPath()
        {
            return _path;
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
