using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmbAbstraction.Tests.Integration.Directory
{
    public class UncPathTests : DirectoryTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture) : base(fixture) { }
    }

    public class SmbUriTests : DirectoryTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture) : base(fixture) { }
    }
}
