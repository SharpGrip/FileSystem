using SharpGrip.FileSystem.Extensions;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Extensions
{
    public class StringExtensionsTest
    {
        [Fact]
        public void Test_IsNullOrEmpty()
        {
            Assert.True("".IsNullOrEmpty());
        }

        [Fact]
        public void Test_RemoveLeadingForwardSlash()
        {
            Assert.Equal("test/", "/test/".RemoveLeadingForwardSlash());
            Assert.Equal("test/", "test/".RemoveLeadingForwardSlash());
        }

        [Fact]
        public void Test_RemoveTrailingForwardSlash()
        {
            Assert.Equal("/test", "/test/".RemoveTrailingForwardSlash());
            Assert.Equal("/test", "/test".RemoveTrailingForwardSlash());
        }

        [Fact]
        public void Test_EnsureTrailingForwardSlash()
        {
            Assert.Equal("/test/", "/test/".EnsureTrailingForwardSlash());
            Assert.Equal("/test/", "/test".EnsureTrailingForwardSlash());
        }
    }
}