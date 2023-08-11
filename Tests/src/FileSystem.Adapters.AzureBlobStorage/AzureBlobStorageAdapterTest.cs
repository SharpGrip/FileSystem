using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.AzureBlobStorage;
using Xunit;

namespace Tests.FileSystem.Adapters.AzureBlobStorage
{
    public class AzureBlobStorageAdapterTest
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
    }
}