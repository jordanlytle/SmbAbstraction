using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmbAbstraction.Tests.Integration.FileInfo
{
    public class UncPathTests : FileInfoTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture) : base(fixture) { }
    }

    public class SmbUriTests : FileInfoTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture) : base(fixture) { }
    }

    public class BaseFileSystemTests : FileInfoTests, IClassFixture<BaseFileSystemFixture>
    {
        public BaseFileSystemTests(BaseFileSystemFixture fixture) : base(fixture) { }
    }
}
