using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace SmbAbstraction.Tests.Integration.FileInfo
{
    public class UncPathTests : FileInfoTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class SmbUriTests : FileInfoTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class BaseFileSystemTests : FileInfoTests, IClassFixture<BaseFileSystemFixture>
    {
        public BaseFileSystemTests(BaseFileSystemFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }
}
