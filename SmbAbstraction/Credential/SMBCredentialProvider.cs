using System.Collections.Generic;
using System.Linq;

namespace SmbAbstraction
{
    public class SMBCredentialProvider : ISMBCredentialProvider
    {
        List<ISMBCredential> credentials = new List<ISMBCredential>();
        private static readonly object _credentialsLock = new object();

        public SMBCredentialProvider()
        {
        }

        public ISMBCredential GetSMBCredential(string path)
        {
            lock(_credentialsLock)
            {
                var host = path.Hostname();
                var shareName = path.ShareName();

                var credential = credentials.Where(q => q.Host == host && q.ShareName == shareName).FirstOrDefault();
                if(credential != null)
                {
                    return credential;
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<ISMBCredential> GetSMBCredentials()
        {
            return credentials;
        }

        public void AddSMBCredential(ISMBCredential credential)
        {
            lock(_credentialsLock)
            {
                credentials.Add(credential);
            }
        }

        public void RemoveSMBCredential(ISMBCredential credential)
        {
            lock(_credentialsLock)
            {
                credentials.Remove(credential);
            }
        }
    }
}
