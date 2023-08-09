using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using NSubstitute;
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
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix", "/root-path", amazonS3Client, "bucket");

            Assert.Equal("prefix", amazonS3Adapter.Prefix);
            Assert.Equal("/root-path", amazonS3Adapter.RootPath);
        }

        [Fact]
        public async Task Test_Get_File_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "/root-path-1", amazonS3Client, "bucket-1");

            var getObjectResponse = Substitute.For<GetObjectResponse>();

            getObjectResponse.Key = "test.txt";
            getObjectResponse.ContentLength = 1;
            getObjectResponse.LastModified = new DateTime(1970, 1, 1);

            amazonS3Client.GetObjectAsync("bucket-1", "/root-path-1/test.txt").Returns(getObjectResponse);

            var fileModel = new FileModel
            {
                Name = "test.txt",
                Path = "test.txt",
                Length = 1,
                LastModifiedDateTime = new DateTime(1970, 1, 1)
            };

            var result = await amazonS3Adapter.GetFileAsync("prefix-1://test.txt");

            Assert.Equal(fileModel.Name, result.Name);
            Assert.Equal(fileModel.Path, result.Path);
            Assert.Equal(fileModel.Length, result.Length);
            Assert.Equal(fileModel.LastModifiedDateTime, result.LastModifiedDateTime);
        }
    }
}