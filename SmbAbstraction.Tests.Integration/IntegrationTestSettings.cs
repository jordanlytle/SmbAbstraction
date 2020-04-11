using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SmbAbstraction.Tests.Integration
{
    public class IntegrationTestSettings
    {
        public ShareCredentials ShareCredentials { get; set; }

        public List<Share> Shares { get; set; }

        public string LocalTempFolder { get; set; }
    }

    public class ShareCredentials
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Share
    {
        public string HostName { get; set; }
        public string ShareName { get; set; }

        public List<string> Files { get; set; }
        public List<string> Directories { get; set; }

        public string GetRootPath(PathType pathType)
        {
            switch(pathType)
            {
                case PathType.SmbUri:
                    return $@"smb://{HostName}/{ShareName}";
                case PathType.UncPath:
                    return $@"\\{HostName}\{ShareName}";
                default:
                    throw new ArgumentException($"PathType must be SmbUri or UncPath");
            }
        }
    }

    public enum PathType 
    { 
        SmbUri,
        UncPath
    }

    public static class IntegrationTestSettingsExtensions
    {
        public static void Initialize(this IntegrationTestSettings settings)
        {
            var configuration = new ConfigurationBuilder()
                                      .AddJsonFile("testsettings.json", optional: false)
                                      .Build();

            configuration.GetSection("IntegrationTestSettings").Bind(settings);
        }
    }
}
