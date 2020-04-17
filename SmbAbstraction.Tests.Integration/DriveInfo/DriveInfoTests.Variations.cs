using SmbAbstraction.Tests.Integration.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace SmbAbstraction.Tests.Integration.DriveInfo
{
    public class UncPathTests : DriveInfoTests, IClassFixture<UncPathFixture>
    {
        public UncPathTests(UncPathFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class SmbUriTests : DriveInfoTests, IClassFixture<SmbUriFixture>
    {
        public SmbUriTests(SmbUriFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }

    public class BaseFileSystemTests : DriveInfoTests, IClassFixture<BaseFileSystemFixture>
    {
        public BaseFileSystemTests(BaseFileSystemFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
    }
}
