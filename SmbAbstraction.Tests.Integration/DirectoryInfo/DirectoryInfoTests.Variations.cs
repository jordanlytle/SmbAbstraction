using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmbAbstraction.Tests.Integration.DirectoryInfo
{
    public class UncPathTests: DirectoryInfoTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture) : base(fixture) { }
    }

    public class SmbUriTests : DirectoryInfoTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture) : base(fixture) { }
    }

    public class BaseFileSystemTests : DirectoryInfoTests, IClassFixture<BaseFileSystemFixture>
    {
        public BaseFileSystemTests(BaseFileSystemFixture fixture) : base(fixture) { }
    }
}
