using FakeItEasy;
using SMBLibrary;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace SmbAbstraction.Tests.Path
{
    public class SMBConnectionTests
    {
        private readonly string domain = "domain";
        private readonly string userName = "user";
        private readonly string path = "\\\\host\\sharename";

        public SMBConnectionTests()
        {
        }

        [Fact]
        public void ThrowExceptionForInvalidCredential()
        {
            var domain = "domain";
            var userName = "user";
            var password = "password";
            var ipAddress = IPAddress.Parse("127.0.0.1");

            var credentials = new List<SMBCredential>() {
                new SMBCredential(null, userName, password, path, A.Fake<ISMBCredentialProvider>()),
                new SMBCredential(domain, null, password, path, A.Fake<ISMBCredentialProvider>()),
                new SMBCredential(domain, userName, null, path, A.Fake<ISMBCredentialProvider>())
            };

            foreach(var credential in credentials)
            {
                Assert.Throws<InvalidCredentialException>(() => { SMBConnection.CreateSMBConnection(A.Fake<ISMBClientFactory>(), ipAddress, SMBTransportType.DirectTCPTransport, credential, 0); });
            }
        }
    }
}
