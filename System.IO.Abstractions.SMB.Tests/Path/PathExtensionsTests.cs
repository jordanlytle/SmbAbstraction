using System;
using Xunit;

namespace System.IO.Abstractions.SMB.Tests.Path
{
    public class PathExtensionsTests
    {
        private readonly IPathTestData _smbUriTestData;
        private readonly IPathTestData _uncPathTestData;

        public PathExtensionsTests()
        {
            _smbUriTestData = new SmbUriTestData();
            _uncPathTestData = new UncPathTestData();
        }

        [Fact]
        public void IsSmbReturnsTrueForSmbUrl()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);

                Assert.True(path.IsSmbPath());
            }
        }

        [Fact]
        public void IsSmbReturnsTrueForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);

                Assert.True(path.IsSmbPath());
            }
        }

        [Fact]
        public void GetHostNameReturnsHostForSmbUrl()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var hostName = path.HostName();

                Assert.Equal("host", hostName);
            }
        }

        [Fact]
        public void GetHostNameReturnsHostForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var hostName = path.HostName();

                Assert.Equal("host", hostName);
            }
        }

        [Fact]
        public void GetSharePathReturnsSharePathForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var sharePath = path.SharePath();
                Assert.Equal(_smbUriTestData.Root, sharePath);
            }
        }

        [Fact]
        public void GetSharePathReturnsSharePathForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var sharePath = path.SharePath();
                Assert.Equal(_uncPathTestData.Root, sharePath);
            }
        }

        [Fact]
        public void GetShareNameReturnsShareForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var shareName = path.ShareName();

                Assert.Equal("share", shareName);
            }
        }

        [Fact]
        public void GetShareNameReturnsShareForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var shareName = path.ShareName();

                Assert.Equal("share", shareName);
            }
        }

        [Fact]
        public void GetRelativeSharePathReturnsPathAfterShareRootForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var relative = ReplacePathSeperators(path.Replace(_smbUriTestData.Root, ""), @"\");
                var relativeSharePath = path.RelativeSharePath();

                Assert.Equal(relative, relativeSharePath);
            }
        }

        [Fact]
        public void GetRelativeSharePathReturnsPathAfterShareRootForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var relative = ReplacePathSeperators(path.Replace(_uncPathTestData.Root, ""), @"\");
                var relativeSharePath = path.RelativeSharePath();

                Assert.Equal(relative, relativeSharePath);
            }
        }

        private string ReplacePathSeperators(string input, string newValue)
        {
            string[] pathSeperators = { @"\", @"/" };

            foreach (var pathSeperator in pathSeperators)
            {
                input = input.Replace(pathSeperator, newValue);
            }

            return input;
        }
    }
}
