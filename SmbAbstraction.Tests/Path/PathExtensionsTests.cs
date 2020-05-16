using Xunit;

namespace SmbAbstraction.Tests.Path
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
        public void IsSharePath_ReturnsFalse_ForLocalUrl()
        {
            string path = "C:\\jordan\\lytle";
            Assert.False(path.IsSharePath());
        }

        [Fact]
        public void IsSharePath_ReturnsTrue_ForSmbUrl()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);

                Assert.True(path.IsSharePath());
            }
        }

        [Fact]
        public void IsSharePath_ReturnsTrue_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);

                Assert.True(path.IsSharePath());
            }
        }

        [Fact]
        public void IsSmbUri_ReturnsTrue_ForSmbUrl()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);

                Assert.True(path.IsSmbUri());
            }
        }

        [Fact]
        public void IsSmbUri_ReturnsFalse_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);

                Assert.False(path.IsSmbUri());
            }
        }

        [Fact]
        public void IsUncPath_ReturnsTrue_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);

                Assert.True(path.IsUncPath());
            }
        }

        [Fact]
        public void IsUncPath_ReturnsFalse_ForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);

                Assert.False(path.IsUncPath());
            }
        }

        [Fact]
        public void BuildSharePath_ReturnsSmbPath_ForSmbPath()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var testBuildShareName = "TestBuildSharePath";

                var builtSharePath = path.BuildSharePath(testBuildShareName);

                string expectedPath = $"smb://{path.Hostname()}/{testBuildShareName}";

                Assert.Equal(expectedPath, builtSharePath);
            }
        }

        [Fact]
        public void BuildSharePath_ReturnsSmbPath_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var testBuildShareName = "TestBuildSharePath";

                var builtSharePath = path.BuildSharePath(testBuildShareName);

                string expectedPath = $@"\\{path.Hostname()}\{testBuildShareName}";
                
                Assert.Equal(expectedPath, builtSharePath);
            }
        }

        [Fact]
        public void HostName_ReturnsHost_ForSmbUrl()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var hostName = path.Hostname();

                Assert.Equal("host", hostName);
            }
        }

        [Fact]
        public void HostName_ReturnsHost_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var hostName = path.Hostname();

                Assert.Equal("host", hostName);
            }
        }


        [Fact]
        public void SharePath_ReturnsSharePath_ForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var sharePath = path.SharePath();
                Assert.Equal(_smbUriTestData.Root, sharePath);
            }
        }

        [Fact]
        public void SharePath_ReturnsSharePath_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var sharePath = path.SharePath();
                Assert.Equal(_uncPathTestData.Root, sharePath);
            }
        }

        [Fact]
        public void ShareName_ReturnsShare_ForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var shareName = path.ShareName();

                Assert.Equal("share", shareName);
            }
        }

        [Fact]
        public void ShareName_ReturnsShare_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var shareName = path.ShareName();

                Assert.Equal("share", shareName);
            }
        }

        [Fact]
        public void RelativeSharePath_ReturnsPathAfterShareRoot_ForSmbUri()
        {
            foreach (var property in _smbUriTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_smbUriTestData);
                var relative = RemoveTrailingSeperator(RemoveLeadingSeperator(ReplacePathSeperators(path.Replace(_smbUriTestData.Root, ""), @"\")));
                var relativeSharePath = path.RelativeSharePath();

                Assert.Equal(relative, relativeSharePath);
            }
        }

        [Fact]
        public void RelativeSharePath_ReturnsPathAfterShareRoot_ForUncPath()
        {
            foreach (var property in _uncPathTestData.GetType().GetProperties())
            {
                var path = (string)property.GetValue(_uncPathTestData);
                var relative = RemoveTrailingSeperator(RemoveLeadingSeperator(ReplacePathSeperators(path.Replace(_uncPathTestData.Root, ""), @"\")));
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

        private string RemoveLeadingSeperator(string input)
        {
            string[] pathSeperators = { @"\", @"/" };

            foreach (var pathSeperator in pathSeperators)
            {
                if (input.StartsWith(pathSeperator))
                {
                    input = input.Remove(0, 1);
                }
            }

            return input;
        }

        private string RemoveTrailingSeperator(string input)
        {
            string[] pathSeperators = { @"\", @"/" };

            foreach (var pathSeperator in pathSeperators)
            {
                if (input.EndsWith(pathSeperator))
                {
                    input = input.Remove(input.LastIndexOf(pathSeperator), 1);
                }
            }

            return input;
        }
    }
}
