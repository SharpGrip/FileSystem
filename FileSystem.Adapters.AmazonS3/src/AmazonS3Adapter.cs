using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.AmazonS3
{
    public class AmazonS3Adapter : Adapter
    {
        private readonly IAmazonS3 client;
        private readonly string bucketName;

        public AmazonS3Adapter(string prefix, string rootPath, IAmazonS3 client, string bucketName) : base(prefix, rootPath)
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

        public override async Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);

            try
            {
                using var response = await client.GetObjectAsync(bucketName, path, cancellationToken);

                return ModelFactory.CreateFile(response, path, virtualPath);
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

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);

            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            try
            {
                if (path == "/")
                {
                    return ModelFactory.CreateDirectory(new S3Object {Key = "/"}, virtualPath);
                }

                var request = new ListObjectsV2Request {BucketName = bucketName, Prefix = path};
                var response = await client.ListObjectsV2Async(request, cancellationToken);

                if (response.KeyCount == 0)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                foreach (var item in response.S3Objects)
                {
                    if (item.Key == path)
                    {
                        return ModelFactory.CreateDirectory(item, virtualPath);
                    }
                }

                throw new DirectoryNotFoundException(path, Prefix);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            try
            {
                var request = new ListObjectsV2Request {BucketName = bucketName, Prefix = path};
                var response = await client.ListObjectsV2Async(request, cancellationToken);

                var files = new List<IFile>();

                foreach (var item in response.S3Objects)
                {
                    var itemName = item.Key.Substring(0, item.Key.Length - path.Length);

                    if (!item.Key.EndsWith("/") && !itemName.Contains('/'))
                    {
                        files.Add(ModelFactory.CreateFile(item, GetVirtualPath(item.Key)));
                    }
                }

                return files;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            try
            {
                var request = new ListObjectsV2Request {BucketName = bucketName, Prefix = path};
                var response = await client.ListObjectsV2Async(request, cancellationToken);

                var directories = new List<IDirectory>();

                foreach (var item in response.S3Objects)
                {
                    var itemName = item.Key.Substring(path.Length).RemoveLeadingForwardSlash();

                    if (item.Key.EndsWith("/") && itemName.Count(c => c.Equals('/')) == 1)
                    {
                        directories.Add(ModelFactory.CreateDirectory(item, GetVirtualPath(item.Key)));
                    }
                }

                return directories;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            if (await DirectoryExistsAsync(virtualPath, cancellationToken))
            {
                throw new DirectoryExistsException(GetPath(virtualPath), Prefix);
            }

            var path = GetPath(virtualPath);
            path = path.EndsWith("/") ? path : path + "/";

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

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath);
            path = path.EndsWith("/") ? path : path + "/";

            try
            {
                var deleteObjectsRequest = new DeleteObjectsRequest {BucketName = bucketName};
                var listObjectsRequest = new ListObjectsV2Request {BucketName = bucketName, Prefix = path};

                var response = await client.ListObjectsV2Async(listObjectsRequest, cancellationToken);

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

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            try
            {
                await client.DeleteObjectAsync(bucketName, path, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            try
            {
                var response = await client.GetObjectAsync(bucketName, path, cancellationToken);

                return response.ResponseStream;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!overwrite && await FileExistsAsync(virtualPath, cancellationToken))
            {
                throw new FileExistsException(GetPath(virtualPath), Prefix);
            }

            var path = GetPath(virtualPath);

            try
            {
                var request = new PutObjectRequest
                {
                    InputStream = contents,
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

        protected override Exception Exception(Exception exception)
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