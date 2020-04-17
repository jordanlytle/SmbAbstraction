using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace SmbAbstraction.Tests.Integration.Directory
{
    public class UncPathTests : DirectoryTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class SmbUriTests : DirectoryTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class BaseFileSystemTests : DirectoryTests, IClassFixture<BaseFileSystemFixture>
    {
        public BaseFileSystemTests(BaseFileSystemFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }
}
