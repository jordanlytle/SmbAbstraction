using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SmbAbstraction.Tests.Integration.Fixtures
{
    public class SmbUriFixture : TestFixture
    {

        private readonly IntegrationTestSettings _settings = new IntegrationTestSettings();

        public SmbUriFixture() : base()
        {
            _settings.Initialize();
        }


        public override string LocalTempDirectory
        {
            get
            {
                if (!string.IsNullOrEmpty(_settings.LocalTempFolder))
                {
                    return _settings.LocalTempFolder;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $@"C:\temp";
                }
                else
                {
                    return $@"{Environment.GetEnvironmentVariable("HOME")}/";
                }
            }
        }

        public override ShareCredentials ShareCredentials => _settings.ShareCredentials;

        public override string ShareName => RootPath.ShareName();
        public override string RootPath => _settings.Shares.First().GetRootPath(PathType.SmbUri);

        public override List<string> Files => _settings.Shares.First().Files;

        public override List<string> Directories => _settings.Shares.First().Directories;

        public override PathType PathType => PathType.SmbUri;
    }
}
