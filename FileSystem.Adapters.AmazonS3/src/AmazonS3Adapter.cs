using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.AmazonS3
{
    public class AmazonS3Adapter : Adapter
    {
        private readonly AmazonS3Client client;
        private readonly string bucketName;

        public AmazonS3Adapter(string prefix, string rootPath, AmazonS3Client client, string bucketName) : base(prefix, rootPath)
        {
            this.client = client;
            this.bucketName = bucketName;
        }

        public override void Dispose()
        {
            client.Dispose();
        }

        public override void Connect()
        {
        }

        public override async Task<IFile> GetFileAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);

            try
            {
                using var response = await client.GetObjectAsync(bucketName, path, cancellationToken);

                return ModelFactory.CreateFile(response);
            }
            catch (AmazonS3Exception exception)
            {
                if (exception.ErrorCode == "NoSuchKey")
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                throw Exception(exception);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);

            if (!path.EndsWith('/'))
            {
                path += "/";
            }

            try
            {
                var prefix = "";
                var pathParts = GetPathParts(path);

                if (pathParts.Length > 1)
                {
                    prefix = string.Join('/', pathParts.SkipLast(1)) + "/";
                }

                var request = new ListObjectsV2Request {BucketName = bucketName, Prefix = prefix};
                var response = await client.ListObjectsV2Async(request, cancellationToken);

                if (response.KeyCount == 0)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                foreach (var item in response.S3Objects)
                {
                    if (item.Key == path)
                    {
                        return ModelFactory.CreateDirectory(item);
                    }
                }

                throw new DirectoryNotFoundException(path, Prefix);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string path = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                var request = new ListObjectsV2Request {BucketName = bucketName, Prefix = path};
                var response = await client.ListObjectsV2Async(request, cancellationToken);

                var files = new List<IFile>();

                foreach (var item in response.S3Objects)
                {
                    var itemName = item.Key.Substring(0, item.Key.Length - path.Length);

                    if (!item.Key.EndsWith('/') && !itemName.Contains('/'))
                    {
                        files.Add(ModelFactory.CreateFile(item));
                    }
                }

                return files;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(
            string path = "",
            CancellationToken cancellationToken = default
        )
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                var request = new ListObjectsV2Request {BucketName = bucketName, Prefix = path};
                var response = await client.ListObjectsV2Async(request, cancellationToken);

                var directories = new List<IDirectory>();

                foreach (var item in response.S3Objects)
                {
                    var itemName = item.Key.Substring(0, item.Key.Length - path.Length);

                    if (item.Key.EndsWith('/') && itemName.Count(c => c.Equals('/')) == 1)
                    {
                        directories.Add(ModelFactory.CreateDirectory(item));
                    }
                }

                return directories;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            if (await DirectoryExistsAsync(path, cancellationToken))
            {
                throw new DirectoryExistsException(PrependRootPath(path), Prefix);
            }

            path = PrependRootPath(path);
            path = path.EndsWith('/') ? path : path + "/";

            try
            {
                var request = new PutObjectRequest {BucketName = bucketName, Key = path, InputStream = new MemoryStream()};
                await client.PutObjectAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);

            path = PrependRootPath(path);
            path = path.EndsWith('/') ? path : path + "/";

            try
            {
                var deleteObjectsRequest = new DeleteObjectsRequest {BucketName = bucketName};
                var listObjectsRequest = new ListObjectsRequest {BucketName = bucketName, Prefix = path};

                var response = await client.ListObjectsAsync(listObjectsRequest, cancellationToken);

                foreach (S3Object entry in response.S3Objects)
                {
                    deleteObjectsRequest.AddKey(entry.Key);
                }

                await client.DeleteObjectsAsync(deleteObjectsRequest, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                await client.DeleteObjectAsync(bucketName, path, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                using var response = await client.GetObjectAsync(bucketName, path, cancellationToken);

                await using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                using var response = await client.GetObjectAsync(bucketName, path, cancellationToken);

                await using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);

                using var streamReader = new StreamReader(memoryStream);
                memoryStream.Position = 0;

                return await streamReader.ReadToEndAsync();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task WriteFileAsync(
            string path,
            byte[] contents,
            bool overwrite = false,
            CancellationToken cancellationToken = default
        )
        {
            if (!overwrite && await FileExistsAsync(path, cancellationToken))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            path = PrependRootPath(path);

            try
            {
                await using var memoryStream = new MemoryStream(contents);
                var request = new PutObjectRequest
                {
                    InputStream = memoryStream,
                    BucketName = bucketName,
                    Key = path
                };

                await client.PutObjectAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            var existingContents = await ReadFileAsync(path, cancellationToken);
            contents = existingContents.Concat(contents).ToArray();
            await DeleteFileAsync(path, cancellationToken);

            path = PrependRootPath(path);

            try
            {
                await using var memoryStream = new MemoryStream(contents);
                var request = new PutObjectRequest
                {
                    InputStream = memoryStream,
                    BucketName = bucketName,
                    Key = path
                };

                await client.PutObjectAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        private static Exception Exception(Exception exception)
        {
            if (exception is FileSystemException)
            {
                return exception;
            }

            if (exception is AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode == "InvalidAccessKeyId" || amazonS3Exception.ErrorCode == "InvalidSecurity")
                {
                    return new ConnectionException(exception);
                }
            }

            return new AdapterRuntimeException(exception);
        }
    }
}