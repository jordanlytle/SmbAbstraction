using System;
using System.Collections.Generic;
using System.Text;

namespace SmbAbstraction.Tests.Integration.Fixtures
{
    public class UncPathFixture : TestFixture
    {
        public UncPathFixture() : base()
        { }

        public override PathType PathType => PathType.UncPath;
    }
}
