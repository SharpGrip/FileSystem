using System.Threading.Tasks;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.Dropbox
{
    public class DropboxAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
        }

        [Fact]
        public Task Test_Connect()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_Directory_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_Files_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_Directories_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_File_Exists_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Directory_Exists_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Create_Directory_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Delete_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Delete_Directory_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Read_File_Stream_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Read_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Read_Text_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Write_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Append_File_Async()
        {
            return Task.CompletedTask;
        }
    }
}