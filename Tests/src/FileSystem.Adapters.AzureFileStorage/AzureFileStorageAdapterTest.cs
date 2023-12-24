using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.AzureFileStorage;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.AzureFileStorage
{
    public class AzureFileStorageAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
            var shareClient = Substitute.For<ShareClient>();
            var azureFileStorageAdapter = new AzureFileStorageAdapter("prefix", "/root-path", shareClient);

            Assert.Equal("prefix", azureFileStorageAdapter.Prefix);
            Assert.Equal("/root-path", azureFileStorageAdapter.RootPath);
        }

        [Fact]
        public Task Test_Connect()
        {
            var shareClient = Substitute.For<ShareClient>();
            var azureFileStorageAdapter = new AzureFileStorageAdapter("prefix", "/root-path", shareClient);

            azureFileStorageAdapter.Connect();

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