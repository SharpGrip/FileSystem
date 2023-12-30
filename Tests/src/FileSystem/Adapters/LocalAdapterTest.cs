using System.Threading.Tasks;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Exceptions;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters
{
    public class LocalAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
            var localAdapter = new LocalAdapter("prefix", "/root-path");

            Assert.Equal("prefix", localAdapter.Prefix);
            Assert.Equal("/root-path", localAdapter.RootPath);
        }

        [Fact]
        public async Task Test_Connect()
        {
            await Task.CompletedTask;
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

        [Fact]
        public async Task Test_File_Exists_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            Assert.False(await localAdapter.FileExistsAsync("prefix-1://test.txt"));
        }

        [Fact]
        public async Task Test_Directory_Exists_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            Assert.False(await localAdapter.DirectoryExistsAsync("prefix-1://test"));
        }

        [Fact]
        public async Task Test_Create_Directory_Async()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Test_Delete_File_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            await Assert.ThrowsAsync<FileNotFoundException>(async () => await localAdapter.DeleteFileAsync("prefix-1://test.txt"));
        }

        [Fact]
        public async Task Test_Delete_Directory_Async()
        {
            var localAdapter = new LocalAdapter("prefix-1", "/root-path-1");

            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await localAdapter.DeleteDirectoryAsync("prefix-1://test"));
        }

        [Fact]
        public async Task Test_Read_File_Stream_Async()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Test_Read_File_Async()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Test_Read_Text_File_Async()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Test_Write_File_Async()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Test_Append_File_Async()
        {
            await Task.CompletedTask;
        }
    }
}