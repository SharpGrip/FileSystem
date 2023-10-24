using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.GoogleDrive;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.GoogleDrive
{
    public class GoogleDriveAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
            var googleDriveClient = Substitute.For<DriveService>();
            var googleDriveAdapter = new GoogleDriveAdapter("prefix", "/root-path", googleDriveClient);

            Assert.Equal("prefix", googleDriveAdapter.Prefix);
            Assert.Equal("/root-path", googleDriveAdapter.RootPath);
        }

        [Fact]
        public Task Test_Connect()
        {
            var googleDriveClient = Substitute.For<DriveService>();
            var googleDriveAdapter = new GoogleDriveAdapter("prefix", "/root-path", googleDriveClient);

            googleDriveAdapter.Connect();

            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_File_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Get_Directory_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Get_Files_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Get_Directories_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_File_Exists_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Directory_Exists_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Create_Directory_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Delete_File_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Delete_Directory_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Read_File_Stream_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Read_File_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Read_Text_File_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Write_File_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }

        [Fact]
        public Task Test_Append_File_Async()
        {
            return Task.FromResult(Task.CompletedTask);
        }
    }
}