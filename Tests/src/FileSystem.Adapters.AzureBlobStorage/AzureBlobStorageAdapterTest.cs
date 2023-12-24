using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.AzureBlobStorage;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.AzureBlobStorage
{
    public class AzureBlobStorageAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
            var blobContainerClient = Substitute.For<BlobContainerClient>();
            var azureBlobStorageAdapter = new AzureBlobStorageAdapter("prefix", "/root-path", blobContainerClient);

            Assert.Equal("prefix", azureBlobStorageAdapter.Prefix);
            Assert.Equal("/root-path", azureBlobStorageAdapter.RootPath);
        }

        [Fact]
        public Task Test_Connect()
        {
            var blobContainerClient = Substitute.For<BlobContainerClient>();
            var azureBlobStorageAdapter = new AzureBlobStorageAdapter("prefix", "/root-path", blobContainerClient);

            azureBlobStorageAdapter.Connect();

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