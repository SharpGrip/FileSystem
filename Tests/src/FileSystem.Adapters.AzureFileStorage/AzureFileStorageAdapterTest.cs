using Azure.Storage.Files.Shares;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.AzureFileStorage;
using Xunit;

namespace Tests.FileSystem.Adapters.AzureFileStorage
{
    public class AzureFileStorageAdapterTest
    {
        [Fact]
        public void Test_Instantiation()
        {
            var shareClient = Substitute.For<ShareClient>();
            var azureFileStorageAdapter = new AzureFileStorageAdapter("prefix", "/root-path", shareClient);

            Assert.Equal("prefix", azureFileStorageAdapter.Prefix);
            Assert.Equal("/root-path", azureFileStorageAdapter.RootPath);
        }
    }
}