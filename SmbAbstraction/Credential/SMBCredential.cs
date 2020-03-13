using System.Collections.Generic;

namespace SmbAbstraction
{
    public class SMBCredential : ISMBCredential
    {
        private readonly ISMBCredentialProvider _credentialProvider;
        public string Host { get; }
        public string ShareName { get; }

        public string Domain { get; } = string.Empty;
        public string UserName { get; }
        public string Password { get; }
        public string Path { get; }

        public SMBCredential(string domain, string userName, string password, string path, ISMBCredentialProvider credentialProvider)
        {
            Domain = domain;
            UserName = userName;
            Password = password;
            Path = path;
            _credentialProvider = credentialProvider;

            Host = path.Hostname();
            ShareName = path.ShareName();

            if(string.IsNullOrEmpty(Domain) && UserName.Contains('\\'))
            {
                var userNameParts = UserName.Split('\\');
                if(userNameParts.Length == 2)
                {
                    Domain = userNameParts[0];
                    UserName = userNameParts[1];
                }
            }

            credentialProvider.AddSMBCredential(this);
        }

        public void Dispose()
        {
            _credentialProvider.RemoveSMBCredential(this);
        }
    }
}
