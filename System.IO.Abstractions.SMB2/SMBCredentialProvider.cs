using System;
using System.Collections.Generic;
using System.Linq;

namespace System.IO.Abstractions.SMB
{
    public class SMBCredentialProvider : ISMBCredentialProvider
    {
        List<Tuple<string, ISMBCredential>> credentials = new List<Tuple<string, ISMBCredential>>();

        public SMBCredentialProvider()
        {
        }

        public ISMBCredential GetSMBCredential(string path)
        {
            var credential = credentials.Where(q => q.Item1 == path).FirstOrDefault();
            if(credential != null)
            {
                return credential.Item2;
            }
            else
            {
                return null;
            }
        }

        public void RemoveSMBCredential(string path, ISMBCredential credential)
        {
            var credentialToRemove = credentials.Where(q => q.Item2.GetUID() == credential.GetUID() && q.Item1 == path).FirstOrDefault();
            if(credentialToRemove != null)
            {
                credentials.Remove(credentialToRemove);
            }
        }

        public void SetSMBCredential(string path, ISMBCredential credential)
        {
            credentials.Add(new Tuple<string, ISMBCredential>(path, credential));
        }
    }
}
