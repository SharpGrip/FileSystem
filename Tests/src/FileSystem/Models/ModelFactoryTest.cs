using System;
using System.IO;
using SharpGrip.FileSystem.Models;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Models
{
    public class ModelFactoryTest
    {
        [Fact]
        public void Test_CreateFile()
        {
            var lastModifiedDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var createdDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

            var file = new FileInfo("../../../var/data/test.txt")
            {
                LastWriteTime = lastModifiedDateTime,
                CreationTime = createdDateTime
            };

            var fileModel = ModelFactory.CreateFile(file, "test://test.txt");

            Assert.Equal("test.txt", fileModel.Name);
            Assert.Contains(@"\var\data\test.txt", fileModel.Path);
            Assert.Equal("test://test.txt", fileModel.VirtualPath);
            Assert.Equal(lastModifiedDateTime, fileModel.LastModifiedDateTime);
            Assert.Equal(createdDateTime, fileModel.CreatedDateTime);
            Assert.Equal(0, fileModel.Length);
        }

        [Fact]
        public void Test_CreateDirectory()
        {
            var lastModifiedDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var createdDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

            var directory = new DirectoryInfo("../../../var/data")
            {
                LastWriteTime = lastModifiedDateTime,
                CreationTime = createdDateTime
            };

            var directoryModel = ModelFactory.CreateDirectory(directory, "test://data");

            Assert.Equal("data", directoryModel.Name);
            Assert.Contains(@"\var\data", directoryModel.Path);
            Assert.Equal("test://data", directoryModel.VirtualPath);
            Assert.Equal(lastModifiedDateTime, directoryModel.LastModifiedDateTime);
            Assert.Equal(createdDateTime, directoryModel.CreatedDateTime);
        }
    }
}