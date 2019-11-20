using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Abstractions.SMB.Tests.Integration
{
    public class IntegrationTestSettings
    {
        public ShareCredentials ShareCredentials {get; set;}

        public List<Share> Shares { get; set; }
    }

    public class ShareCredentials
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Share
    {
        public string RootUncPath { get; set; }
        public string RootSmbUri { get; set; }

        public List<string> Files { get; set; }
        public List<string> Directories { get; set; }
    }
    public static class IntegrationTestSettingsExtensions
    {
        public static void Initialize(this IntegrationTestSettings settings)
        {
            var configuration = new ConfigurationBuilder()
                                      .AddJsonFile("testsettings.json", optional: false)
                                      .AddJsonFile("testsettings.development.json", optional: true)
                                      .Build();

            configuration.GetSection("IntegrationTestSettings").Bind(settings);
        }
    }
}
