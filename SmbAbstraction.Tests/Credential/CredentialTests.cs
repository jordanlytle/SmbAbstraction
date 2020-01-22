using Xunit;
using FakeItEasy;

namespace SmbAbstraction.Tests.Path
{
    public class CredentialTests
    {
        private readonly string domain = "domain";
        private readonly string userName = "user";
        private readonly string path = "\\\\host\\sharename";

        public CredentialTests()
        {
        }

        [Fact]
        public void SetDomainNameFromUserNameIfNull()
        {
            var credential = new SMBCredential(null, $"{domain}\\{userName}", "password", path, A.Fake<ISMBCredentialProvider>()); 
            Assert.Equal(domain, credential.Domain);
            Assert.Equal(userName, credential.UserName);
        }

        [Fact]
        public void DoNotSetDomainNameFromUserNameIfNotNull()
        {
            var domain = "domain";
            var userName = "user";
            var combinedUserName = $"{domain}\\{userName}";

            var credential = new SMBCredential(domain, combinedUserName, "password", path, A.Fake<ISMBCredentialProvider>()); 
            Assert.Equal(domain, credential.Domain);
            Assert.Equal(combinedUserName, credential.UserName);
        }
    }
}
