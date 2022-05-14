using System.Threading.Tasks;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Exceptions;
using Xunit;

namespace Tests.FileSystem
{
    public class LocalAdapterTest
    {
        [Fact]
        public void Test_Instantiation()
        {
            var localAdapter = new LocalAdapter("prefix", "/root-path");

            Assert.Equal("prefix", localAdapter.Prefix);
            Assert.Equal("/root-path", localAdapter.RootPath);
        }

        [Fact]
        public async Task Test_Get_File_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            await Assert.ThrowsAsync<FileNotFoundException>(async () => await localAdapter.GetFileAsync("prefix-1://test.txt"));
        }

        [Fact]
        public async Task Test_Get_Directory_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await localAdapter.GetDirectoryAsync("prefix-1://test"));
        }

        [Fact]
        public async Task Test_Get_Files_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await localAdapter.GetFilesAsync("prefix-1://test"));
        }

        [Fact]
        public async Task Test_Get_Directories_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await localAdapter.GetDirectoriesAsync("prefix-1://test"));
        }
    }
}