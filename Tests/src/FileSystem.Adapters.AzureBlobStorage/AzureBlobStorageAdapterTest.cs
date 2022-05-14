using Azure.Storage.Blobs;
using Moq;
using SharpGrip.FileSystem.Adapters.AzureBlobStorage;
using Xunit;

namespace Tests.FileSystem.Adapters.AzureBlobStorage;

public class AzureBlobStorageAdapterTest
{
    [Fact]
    public void Test_Instantiation()
    {
        var blobContainerClient = new Mock<BlobContainerClient>();
        var azureBlobStorageAdapter = new AzureBlobStorageAdapter("prefix", "/root-path", blobContainerClient.Object);

        Assert.Equal("prefix", azureBlobStorageAdapter.Prefix);
        Assert.Equal("/root-path", azureBlobStorageAdapter.RootPath);
    }
}