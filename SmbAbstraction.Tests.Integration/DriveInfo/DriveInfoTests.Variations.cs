using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmbAbstraction.Tests.Integration.DriveInfo
{
    public class UncPathTests : DriveInfoTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture) : base(fixture) { }
    }

    public class SmbUriTests : DriveInfoTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture) : base(fixture) { }
    }
}
