using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmbAbstraction.Tests.Integration.File
{
    public class UncPathTests : FileTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture) : base(fixture) { }
    }

    public class SmbUriTests : FileTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture) : base(fixture) { }
    }
}
