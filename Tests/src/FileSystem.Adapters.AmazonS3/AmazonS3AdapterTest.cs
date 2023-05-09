using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using SharpGrip.FileSystem.Adapters.AmazonS3;
using SharpGrip.FileSystem.Models;
using Xunit;

namespace Tests.FileSystem.Adapters.AmazonS3
{
    public class AmazonS3AdapterTest
    {
        [Fact]
        public void Test_Instantiation()
        {
            var amazonS3Client = new Mock<AmazonS3Client>("awsAccessKeyId", "awsSecretAccessKey", RegionEndpoint.USEast2);
            var amazonS3Adapter = new AmazonS3Adapter("prefix", "/root-path", amazonS3Client.Object, "bucket");

            Assert.Equal("prefix", amazonS3Adapter.Prefix);
            Assert.Equal("/root-path", amazonS3Adapter.RootPath);
        }

        [Fact]
        public async Task Test_Get_File_Async()
        {
            var amazonS3Client = new Mock<AmazonS3Client>("awsAccessKeyId", "awsSecretAccessKey", RegionEndpoint.USEast2);
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "/root-path-1", amazonS3Client.Object, "bucket-1");

            var getObjectResponse = new Mock<GetObjectResponse>();

            getObjectResponse.SetupAllProperties();
            getObjectResponse.Object.Key = "test.txt";
            getObjectResponse.Object.ContentLength = 1;
            getObjectResponse.Object.LastModified = new DateTime(1970, 1, 1);

            amazonS3Client.Setup(o => o.GetObjectAsync("bucket-1", "/root-path-1\\test.txt", default)).ReturnsAsync(getObjectResponse.Object);

            var fileModel = new FileModel
            {
                Name = "test.txt",
                Path = "test.txt",
                Length = 1,
                LastModifiedDateTime = new DateTime(1970, 1, 1)
            };

            var result = await amazonS3Adapter.GetFileAsync("test.txt");

            Assert.Equal(fileModel.Name, result.Name);
            Assert.Equal(fileModel.Path, result.Path);
            Assert.Equal(fileModel.Length, result.Length);
            Assert.Equal(fileModel.LastModifiedDateTime, result.LastModifiedDateTime);
        }
    }
}