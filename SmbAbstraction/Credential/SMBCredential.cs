using System;
using System.Collections.Generic;

namespace SmbAbstraction
{
    public class SMBCredential : ISMBCredential
    {
        public string Host { get; }
        public string ShareName { get; }

        public string Domain { get; }
        public string UserName { get; }
        public string Password { get; }
        public string Path { get; }

        private List<ISMBCredential> _parentList;

        public SMBCredential(string domain, string userName, string password, string path)
        {
            Domain = domain;
            UserName = userName;
            Password = password;
            Path = path;

            Host = path.Hostname();
            ShareName = path.ShareName();
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
