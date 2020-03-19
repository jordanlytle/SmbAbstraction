using System;
using System.Collections.Generic;
using System.Text;

namespace SmbAbstraction.Tests.Integration.Fixtures
{
    public class SmbUriFixture : TestFixture
    {
        public SmbUriFixture() : base()
        { }

        public override PathType PathType => PathType.SmbUri;
    }
}
