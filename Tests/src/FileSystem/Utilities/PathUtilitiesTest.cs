using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Utilities;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Utilities
{
    public class PathUtilitiesTest
    {
        [Fact]
        public void Test_GetPrefix()
        {
            Assert.Equal("prefix", PathUtilities.GetPrefix("prefix://test.txt"));
            Assert.Throws<PrefixNotFoundInPathException>(() => PathUtilities.GetPrefix("test.txt"));
        }

        [Fact]
        public void Test_NormalizeRootPath()
        {
            Assert.Equal("/", PathUtilities.NormalizeRootPath(""));
            Assert.Equal("/", PathUtilities.NormalizeRootPath("/"));
            Assert.Equal("/test1/test2/test3", PathUtilities.NormalizeRootPath(@"\test1\test2\test3\"));
            Assert.Equal("/test1/test2/test3", PathUtilities.NormalizeRootPath(@"\test1\test2\test3"));
        }

        [Fact]
        public void Test_GetPath()
        {
            Assert.Equal("/root-path/virtual-path", PathUtilities.GetPath("prefix://virtual-path", "/root-path"));
            Assert.Equal("/root-path", PathUtilities.GetPath("prefix://", "/root-path"));
            Assert.Equal("", PathUtilities.GetPath("prefix://", ""));
        }

        [Fact]
        public void Test_GetVirtualPath()
        {
            Assert.Equal("prefix://test.txt", PathUtilities.GetVirtualPath("root-path/test.txt", "prefix", "root-path"));
            Assert.Equal("prefix://root-path/test.txt", PathUtilities.GetVirtualPath("root-path/test.txt", "prefix", "/"));
            Assert.Equal("prefix://root-path/test.txt", PathUtilities.GetVirtualPath("root-path/test.txt", "prefix", ""));
            Assert.Throws<InvalidPathException>(() => PathUtilities.GetVirtualPath("test://test.txt", "prefix", "root-path"));
        }

        [Fact]
        public void Test_GetParentPathPart()
        {
            Assert.Equal("", PathUtilities.GetParentPathPart(""));
            Assert.Equal("", PathUtilities.GetParentPathPart("/test-1"));
            Assert.Equal("test-1", PathUtilities.GetParentPathPart("/test-1/test-2"));
            Assert.Equal("test-1/test-2", PathUtilities.GetParentPathPart("/test-1/test-2/test-3"));
        }

        [Fact]
        public void Test_GetLastPathPart()
        {
            Assert.Equal("", PathUtilities.GetLastPathPart(""));
            Assert.Equal("test-1", PathUtilities.GetLastPathPart("/test-1"));
            Assert.Equal("test-2", PathUtilities.GetLastPathPart("/test-1/test-2"));
            Assert.Equal("test-3", PathUtilities.GetLastPathPart("/test-1/test-2/test-3"));
        }

        [Fact]
        public void Test_GetPathParts()
        {
            Assert.Equal(new string[] { }, PathUtilities.GetPathParts(""));
            Assert.Equal(new[] {"test-1"}, PathUtilities.GetPathParts("/test-1"));
            Assert.Equal(new[] {"test-1", "test-2"}, PathUtilities.GetPathParts("/test-1/test-2"));
            Assert.Equal(new[] {"test-1", "test-2", "test-3"}, PathUtilities.GetPathParts("/test-1/test-2/test-3"));
        }
    }
}