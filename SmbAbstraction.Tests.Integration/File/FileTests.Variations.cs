using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace SmbAbstraction.Tests.Integration.File
{
    public class UncPathTests : FileTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class SmbUriTests : FileTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class BaseFileSystemTests : FileTests, IClassFixture<BaseFileSystemFixture>
    {
        public BaseFileSystemTests(BaseFileSystemFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }
}
