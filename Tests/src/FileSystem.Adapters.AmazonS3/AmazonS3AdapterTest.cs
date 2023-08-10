using System;
using System.Collections.Generic;
using System.IO;
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
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace Tests.FileSystem.Adapters.AmazonS3
{
    public class AmazonS3AdapterTest : IAdapterTests
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
        public async Task Test_Connect()
        {
            await Task.CompletedTask;
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
        public async Task Test_Delete_File_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse = Substitute.For<GetObjectResponse>();

            getObjectResponse.Key = "test4.txt";
            getObjectResponse.ContentLength = 1;
            getObjectResponse.LastModified = new DateTime(1970, 1, 1);

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").ThrowsAsync(amazonS3InvalidSecurityException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").Returns(getObjectResponse);

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.DeleteFileAsync("prefix-1://test1.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteFileAsync("prefix-1://test3.txt"));
            await fileSystem.DeleteFileAsync("prefix-1://test4.txt");
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

        [Fact]
        public async Task Test_Read_File_Stream_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse1 = Substitute.For<GetObjectResponse>();
            var getObjectResponse2 = Substitute.For<GetObjectResponse>();

            getObjectResponse1.Key = "test1.txt";
            getObjectResponse1.ContentLength = 1;
            getObjectResponse1.LastModified = new DateTime(1970, 1, 1);

            getObjectResponse2.Key = "test1.txt";
            getObjectResponse2.ContentLength = 1;
            getObjectResponse2.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse2.ResponseStream = new MemoryStream("test1"u8.ToArray());

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse1, getObjectResponse2);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").ThrowsAsync(amazonS3InvalidSecurityException);

            var fileStream = await fileSystem.ReadFileStreamAsync("prefix-1://test1.txt");
            var streamReader = new StreamReader(fileStream);

            Assert.Equal("test1", await streamReader.ReadToEndAsync());
            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test4.txt"));
        }

        [Fact]
        public async Task Test_Read_File_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse1 = Substitute.For<GetObjectResponse>();
            var getObjectResponse2 = Substitute.For<GetObjectResponse>();
            var getObjectResponse3 = Substitute.For<GetObjectResponse>();

            getObjectResponse1.Key = "test1.txt";
            getObjectResponse1.ContentLength = 1;
            getObjectResponse1.LastModified = new DateTime(1970, 1, 1);

            getObjectResponse2.Key = "test1.txt";
            getObjectResponse2.ContentLength = 1;
            getObjectResponse2.LastModified = new DateTime(1970, 1, 1);

            getObjectResponse3.Key = "test1.txt";
            getObjectResponse3.ContentLength = 1;
            getObjectResponse3.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse3.ResponseStream = new MemoryStream("test1"u8.ToArray());

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse1, getObjectResponse2, getObjectResponse3);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").ThrowsAsync(amazonS3InvalidSecurityException);

            var fileContents = await fileSystem.ReadFileAsync("prefix-1://test1.txt");

            Assert.Equal("test1", System.Text.Encoding.UTF8.GetString(fileContents));
            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.ReadFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test4.txt"));
        }

        [Fact]
        public async Task Test_Read_Text_File_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse1 = Substitute.For<GetObjectResponse>();
            var getObjectResponse2 = Substitute.For<GetObjectResponse>();
            var getObjectResponse3 = Substitute.For<GetObjectResponse>();

            getObjectResponse1.Key = "test1.txt";
            getObjectResponse1.ContentLength = 1;
            getObjectResponse1.LastModified = new DateTime(1970, 1, 1);

            getObjectResponse2.Key = "test1.txt";
            getObjectResponse2.ContentLength = 1;
            getObjectResponse2.LastModified = new DateTime(1970, 1, 1);

            getObjectResponse3.Key = "test1.txt";
            getObjectResponse3.ContentLength = 1;
            getObjectResponse3.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse3.ResponseStream = new MemoryStream("test1"u8.ToArray());

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse1, getObjectResponse2, getObjectResponse3);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").ThrowsAsync(amazonS3InvalidSecurityException);

            var fileContents = await fileSystem.ReadTextFileAsync("prefix-1://test1.txt");

            Assert.Equal("test1", fileContents);
            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.ReadFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test4.txt"));
        }

        [Fact]
        public async Task Test_Write_File_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse = Substitute.For<GetObjectResponse>();

            getObjectResponse.Key = "test1.txt";
            getObjectResponse.ContentLength = 1;
            getObjectResponse.LastModified = new DateTime(1970, 1, 1);

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").Returns(getObjectResponse);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").Returns(getObjectResponse);
            amazonS3Client.PutObjectAsync(Arg.Is<PutObjectRequest>(x => x.BucketName == "bucket-1" && x.Key == "root-path-1/test2.txt")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.PutObjectAsync(Arg.Is<PutObjectRequest>(x => x.BucketName == "bucket-1" && x.Key == "root-path-1/test3.txt")).ThrowsAsync(amazonS3InvalidSecurityException);

            await fileSystem.WriteFileAsync("prefix-1://test1.txt", new MemoryStream(), true);
            await Assert.ThrowsAsync<FileExistsException>(() => fileSystem.WriteFileAsync("prefix-1://test1.txt", new MemoryStream()));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.WriteFileAsync("prefix-1://test2.txt", new MemoryStream(), true));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.WriteFileAsync("prefix-1://test3.txt", new MemoryStream(), true));
        }

        [Fact]
        public async Task Test_Append_File_Async()
        {
            var amazonS3Client = Substitute.For<IAmazonS3>();
            var amazonS3Adapter = new AmazonS3Adapter("prefix-1", "root-path-1", amazonS3Client, "bucket-1");
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {amazonS3Adapter});

            var getObjectResponse1 = Substitute.For<GetObjectResponse>();
            var getObjectResponse2 = Substitute.For<GetObjectResponse>();
            var getObjectResponse3 = Substitute.For<GetObjectResponse>();
            var getObjectResponse4 = Substitute.For<GetObjectResponse>();
            var getObjectResponse5 = Substitute.For<GetObjectResponse>();

            getObjectResponse1.Key = "test1.txt";
            getObjectResponse1.ContentLength = 1;
            getObjectResponse1.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse3.ResponseStream = new MemoryStream("test1"u8.ToArray());

            getObjectResponse2.Key = "test1.txt";
            getObjectResponse2.ContentLength = 1;
            getObjectResponse2.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse3.ResponseStream = new MemoryStream("test1"u8.ToArray());

            getObjectResponse3.Key = "test1.txt";
            getObjectResponse3.ContentLength = 1;
            getObjectResponse3.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse3.ResponseStream = new MemoryStream("test1"u8.ToArray());

            getObjectResponse4.Key = "test1.txt";
            getObjectResponse4.ContentLength = 1;
            getObjectResponse4.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse4.ResponseStream = new MemoryStream("test1"u8.ToArray());

            getObjectResponse5.Key = "test1.txt";
            getObjectResponse5.ContentLength = 1;
            getObjectResponse5.LastModified = new DateTime(1970, 1, 1);
            getObjectResponse5.ResponseStream = new MemoryStream("test1"u8.ToArray());

            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test1.txt").Returns(getObjectResponse1, getObjectResponse2, getObjectResponse3, getObjectResponse4, getObjectResponse5);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test2.txt").ThrowsAsync(amazonS3NoSuchKeyException);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test3.txt").Returns(getObjectResponse1, getObjectResponse2, getObjectResponse3, getObjectResponse4, getObjectResponse5);
            amazonS3Client.GetObjectAsync("bucket-1", "root-path-1/test4.txt").Returns(getObjectResponse1, getObjectResponse2, getObjectResponse3, getObjectResponse4, getObjectResponse5);
            amazonS3Client.PutObjectAsync(Arg.Is<PutObjectRequest>(x => x.BucketName == "bucket-1" && x.Key == "root-path-1/test2.txt")).ThrowsAsync(amazonS3InvalidAccessKeyIdException);
            amazonS3Client.PutObjectAsync(Arg.Is<PutObjectRequest>(x => x.BucketName == "bucket-1" && x.Key == "root-path-1/test3.txt")).ThrowsAsync(amazonS3InvalidSecurityException);
            amazonS3Client.PutObjectAsync(Arg.Is<PutObjectRequest>(x => x.BucketName == "bucket-1" && x.Key == "root-path-1/test4.txt")).ThrowsAsync(amazonS3InvalidSecurityException);

            await fileSystem.AppendFileAsync("prefix-1://test1.txt", new MemoryStream());
            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.AppendFileAsync("prefix-1://test2.txt", new MemoryStream()));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.AppendFileAsync("prefix-1://test3.txt", new MemoryStream()));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.AppendFileAsync("prefix-1://test4.txt", new MemoryStream()));
        }
    }
}