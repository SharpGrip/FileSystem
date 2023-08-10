using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Adapters.AmazonS3;
using SharpGrip.FileSystem.Exceptions;
using Xunit;

namespace Tests.FileSystem.Adapters.AmazonS3
{
    public class AmazonS3AdapterTest
    {
        private readonly AmazonS3Exception amazonS3NoSuchKeyException = new("NoSuchKey", ErrorType.Receiver, "NoSuchKey", "12345", HttpStatusCode.NotFound);
        private readonly AmazonS3Exception amazonS3InvalidAccessKeyIdException = new("InvalidAccessKeyId", ErrorType.Receiver, "InvalidAccessKeyId", "12345", HttpStatusCode.Unauthorized);
        private readonly AmazonS3Exception amazonS3InvalidSecurityException = new("InvalidSecurity", ErrorType.Receiver, "InvalidSecurity", "12345", HttpStatusCode.Unauthorized);

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
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse = Substitute.For<GetObjectResponse>();

            getObjectResponse.Key = "test1.txt";
            getObjectResponse.ContentLength = 1;
            getObjectResponse.LastModified = new DateTime(1970, 1, 1);

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").ThrowsAsync(amazonS3InvalidSecurityException);

            var file = await fileSystem.GetFileAsync("prefix-1://test1.txt");

            Assert.Equal("test1.txt", file.Name);
            Assert.Equal("root-path-1/test1.txt", file.Path);
            Assert.Equal("prefix-1://test1.txt", file.VirtualPath);
            Assert.Equal(1, file.Length);
            Assert.Equal(new DateTime(1970, 1, 1), file.LastModifiedDateTime);

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.GetFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test4.txt"));
        }

        [Fact]
        public async Task Test_Get_Directory_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var listObjectsV2Response1 = Substitute.For<ListObjectsV2Response>();
            var listObjectsV2Response2 = Substitute.For<ListObjectsV2Response>();

            var s3Object = new S3Object
            {
                Key = "root-path-1/test1/",
                LastModified = new DateTime(1970, 1, 1)
            };

            listObjectsV2Response1.S3Objects.Add(s3Object);
            listObjectsV2Response1.KeyCount = listObjectsV2Response1.S3Objects.Count;

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test1/")).Returns(listObjectsV2Response1);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test2/")).Returns(listObjectsV2Response2);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test3/")).ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test4/")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test5/")).ThrowsAsync(amazonS3InvalidSecurityException);

            var directory = await fileSystem.GetDirectoryAsync("prefix-1://test1");

            Assert.Equal("test1", directory.Name);
            Assert.Equal("root-path-1/test1", directory.Path);
            Assert.Equal("prefix-1://test1", directory.VirtualPath);
            Assert.Equal(new DateTime(1970, 1, 1), directory.LastModifiedDateTime);

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => fileSystem.GetDirectoryAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.GetDirectoryAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoryAsync("prefix-1://test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoryAsync("prefix-1://test5"));
        }

        [Fact]
        public async Task Test_Get_Files_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var listObjectsV2Response = Substitute.For<ListObjectsV2Response>();

            var s3ObjectFile = new S3Object
            {
                Key = "root-path-1/test1/test.txt",
                Size = 1,
                LastModified = new DateTime(1970, 1, 1)
            };
            var s3ObjectDirectory = new S3Object
            {
                Key = "root-path-1/test1/",
                LastModified = new DateTime(1970, 1, 1)
            };

            listObjectsV2Response.S3Objects.Add(s3ObjectFile);
            listObjectsV2Response.S3Objects.Add(s3ObjectDirectory);
            listObjectsV2Response.KeyCount = listObjectsV2Response.S3Objects.Count;

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test1/")).Returns(listObjectsV2Response);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test2/")).ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test3/")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test4/")).ThrowsAsync(amazonS3InvalidSecurityException);

            var files = await fileSystem.GetFilesAsync("prefix-1://test1");
            var file = files.First();

            Assert.Equal("test.txt", file.Name);
            Assert.Equal("root-path-1/test1/test.txt", file.Path);
            Assert.Equal("prefix-1://test1/test.txt", file.VirtualPath);
            Assert.Equal(1, file.Length);
            Assert.Equal(new DateTime(1970, 1, 1), file.LastModifiedDateTime);

            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.GetFilesAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFilesAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFilesAsync("prefix-1://test4"));
        }

        [Fact]
        public async Task Test_Get_Directories_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var listObjectsV2Response = Substitute.For<ListObjectsV2Response>();

            var s3ObjectDirectory1 = new S3Object
            {
                Key = "root-path-1/",
                Size = 1,
                LastModified = new DateTime(1970, 1, 1)
            };
            var s3ObjectDirectory2 = new S3Object
            {
                Key = "root-path-1/test1/",
                Size = 1,
                LastModified = new DateTime(1970, 1, 1)
            };
            var s3ObjectDirectory3 = new S3Object
            {
                Key = "root-path-1/test2/",
                Size = 1,
                LastModified = new DateTime(1970, 1, 1)
            };

            listObjectsV2Response.S3Objects.Add(s3ObjectDirectory1);
            listObjectsV2Response.S3Objects.Add(s3ObjectDirectory2);
            listObjectsV2Response.S3Objects.Add(s3ObjectDirectory3);
            listObjectsV2Response.KeyCount = listObjectsV2Response.S3Objects.Count;

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/")).Returns(listObjectsV2Response);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test1/")).ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test2/")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test3/")).ThrowsAsync(amazonS3InvalidSecurityException);

            var directories = (await fileSystem.GetDirectoriesAsync("prefix-1://")).ToList();
            var directory1 = directories[0];
            var directory2 = directories[1];

            Assert.Equal("test1", directory1.Name);
            Assert.Equal("root-path-1/test1", directory1.Path);
            Assert.Equal("prefix-1://test1/", directory1.VirtualPath);
            Assert.Equal(new DateTime(1970, 1, 1), directory1.LastModifiedDateTime);

            Assert.Equal("test2", directory2.Name);
            Assert.Equal("root-path-1/test2", directory2.Path);
            Assert.Equal("prefix-1://test2/", directory2.VirtualPath);
            Assert.Equal(new DateTime(1970, 1, 1), directory2.LastModifiedDateTime);

            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test1"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test3"));
        }

        [Fact]
        public async Task Test_File_Exists_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse = Substitute.For<GetObjectResponse>();

            getObjectResponse.Key = "test1.txt";
            getObjectResponse.ContentLength = 1;
            getObjectResponse.LastModified = new DateTime(1970, 1, 1);

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").ThrowsAsync(amazonS3InvalidSecurityException);

            Assert.True(await fileSystem.FileExistsAsync("prefix-1://test1.txt"));
            Assert.False(await fileSystem.FileExistsAsync("prefix-1://test2.txt"));

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.GetFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test4.txt"));
        }

        [Fact]
        public async Task Test_Directory_Exists_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var listObjectsV2Response1 = Substitute.For<ListObjectsV2Response>();
            var listObjectsV2Response2 = Substitute.For<ListObjectsV2Response>();

            var s3Object = new S3Object
            {
                Key = "root-path-1/test1/",
                LastModified = new DateTime(1970, 1, 1)
            };

            listObjectsV2Response1.S3Objects.Add(s3Object);
            listObjectsV2Response1.KeyCount = listObjectsV2Response1.S3Objects.Count;

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test1/")).Returns(listObjectsV2Response1);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test2/")).Returns(listObjectsV2Response2);

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test3/")).ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test4/")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test5/")).ThrowsAsync(amazonS3InvalidSecurityException);

            Assert.True(await fileSystem.DirectoryExistsAsync("prefix-1://"));
            Assert.True(await fileSystem.DirectoryExistsAsync("prefix-1://root-path-1/test1"));
            Assert.False(await fileSystem.DirectoryExistsAsync("prefix-1://root-path-1/test2"));

            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.DirectoryExistsAsync("prefix-1://root-path-1/test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DirectoryExistsAsync("prefix-1://root-path-1/test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DirectoryExistsAsync("prefix-1://root-path-1/test5"));
        }

        [Fact]
        public async Task Test_Create_Directory_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var listObjectsV2Response1 = Substitute.For<ListObjectsV2Response>();
            var listObjectsV2Response2 = Substitute.For<ListObjectsV2Response>();

            var s3Object = new S3Object
            {
                Key = "root-path-1/test1/",
                LastModified = new DateTime(1970, 1, 1)
            };

            listObjectsV2Response1.S3Objects.Add(s3Object);
            listObjectsV2Response1.KeyCount = listObjectsV2Response1.S3Objects.Count;

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test1/")).Returns(listObjectsV2Response1);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test2/")).ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test3/")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test4/")).ThrowsAsync(amazonS3InvalidSecurityException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test5/")).Returns(listObjectsV2Response2);

            await Assert.ThrowsAsync<DirectoryExistsException>(() => fileSystem.CreateDirectoryAsync("prefix-1://test1"));
            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.CreateDirectoryAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.CreateDirectoryAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.CreateDirectoryAsync("prefix-1://test4"));
            await fileSystem.CreateDirectoryAsync("prefix-1://test5");
        }

        [Fact]
        public async Task Test_Delete_Directory_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var listObjectsV2Response1 = Substitute.For<ListObjectsV2Response>();
            var listObjectsV2Response2 = Substitute.For<ListObjectsV2Response>();

            var s3Object = new S3Object
            {
                Key = "root-path-1/test5/",
                LastModified = new DateTime(1970, 1, 1)
            };

            listObjectsV2Response2.S3Objects.Add(s3Object);
            listObjectsV2Response2.KeyCount = listObjectsV2Response2.S3Objects.Count;

            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test1/")).Returns(listObjectsV2Response1);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test2/")).ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test3/")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test4/")).ThrowsAsync(amazonS3InvalidSecurityException);
            amazonS3Client.ListObjectsV2Async(Arg.Is<ListObjectsV2Request>(x => x.Prefix == "root-path-1/test5/")).Returns(listObjectsV2Response2);

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test1"));
            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test4"));
            await fileSystem.DeleteDirectoryAsync("prefix-1://test5");
        }
    }
}