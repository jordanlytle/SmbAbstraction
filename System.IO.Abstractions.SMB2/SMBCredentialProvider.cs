using System;
using System.Collections.Generic;
using System.Linq;

namespace System.IO.Abstractions.SMB
{
    public class SMBCredentialProvider : ISMBCredentialProvider
    {
        List<ISMBCredential> credentials = new List<ISMBCredential>();

        public SMBCredentialProvider()
        {
        }

        public ISMBCredential GetSMBCredential(string path)
        {
            var credential = credentials.Where(q => q.Path == path).FirstOrDefault();
            if(credential != null)
            {
                return credential;
            }
            else
            {
                return null;
            }
        }

        public void AddSMBCredential(ISMBCredential credential)
        {
            credential.SetParentList(credentials);
            credentials.Add(credential);
        }
    }
}
