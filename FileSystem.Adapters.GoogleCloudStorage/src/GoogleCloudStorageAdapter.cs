using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace SharpGrip.FileSystem.Adapters.GoogleCloudStorage
{
    public class GoogleCloudStorageAdapter : Adapter<GoogleCloudStorageAdapterConfiguration, string, string>
    {
        private readonly StorageClient client;
        private readonly string bucketName;

        public GoogleCloudStorageAdapter(string prefix, string rootPath, StorageClient client, string bucketName, Action<GoogleCloudStorageAdapterConfiguration>? configuration = null)
            : base(prefix, rootPath, configuration)
        {
            this.client = client;
            this.bucketName = bucketName;
        }

        public override void Dispose()
        {
            client.Dispose();
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogStartConnectingAdapter(this);
            await Task.CompletedTask;
            Logger.LogFinishedConnectingAdapter(this);
        }

        public override async Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                var file = await client.GetObjectAsync(bucketName, path, new GetObjectOptions(), cancellationToken);

                if (file == null)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file, path, virtualPath);
            }
            catch (GoogleApiException googleApiException) when (googleApiException.HttpStatusCode == HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException(path, Prefix);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();
            var parentPath = GetParentPathPart(path).EnsureTrailingForwardSlash();

            try
            {
                if (path.IsNullOrEmpty() || path == "/")
                {
                    return ModelFactory.CreateDirectory("/", path, virtualPath);
                }

                var request = client.Service.Objects.List(bucketName);

                request.Prefix = parentPath == "/" ? null : parentPath;
                request.Delimiter = "/";

                do
                {
                    var objects = await request.ExecuteAsync(cancellationToken: cancellationToken);

                    if (objects.Prefixes != null)
                    {
                        foreach (var directoryPath in objects.Prefixes)
                        {
                            if (directoryPath == path)
                            {
                                var directoryName = GetLastPathPart(directoryPath);

                                return ModelFactory.CreateDirectory(directoryName.RemoveTrailingForwardSlash(), directoryPath.EnsureTrailingForwardSlash(), GetVirtualPath(directoryPath));
                            }
                        }
                    }

                    request.PageToken = objects.NextPageToken;
                } while (request.PageToken != null);

                throw new DirectoryNotFoundException(path, Prefix);
            }
            catch (GoogleApiException googleApiException) when (googleApiException.HttpStatusCode == HttpStatusCode.NotFound)
            {
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

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                var files = new List<IFile>();

                var request = client.Service.Objects.List(bucketName);

                request.Prefix = path == "/" ? null : path;
                request.Delimiter = "/";

                do
                {
                    var objects = await request.ExecuteAsync(cancellationToken: cancellationToken);

                    if (objects.Items != null)
                    {
                        foreach (var file in objects.Items.Where(item => item.ContentType != null))
                        {
                            files.Add(ModelFactory.CreateFile(file, file.Name.RemoveTrailingForwardSlash(), GetVirtualPath(file.Name)));
                        }
                    }

                    request.PageToken = objects.NextPageToken;
                } while (request.PageToken != null);

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

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                var directories = new List<IDirectory>();

                var request = client.Service.Objects.List(bucketName);

                request.Prefix = path == "/" ? null : path;
                request.Delimiter = "/";

                do
                {
                    var objects = await request.ExecuteAsync(cancellationToken: cancellationToken);

                    if (objects.Prefixes != null)
                    {
                        foreach (var directoryPath in objects.Prefixes)
                        {
                            var directoryName = GetLastPathPart(directoryPath);

                            directories.Add(ModelFactory.CreateDirectory(directoryName.RemoveTrailingForwardSlash(), directoryPath.EnsureTrailingForwardSlash(), GetVirtualPath(directoryPath)));
                        }
                    }

                    request.PageToken = objects.NextPageToken;
                } while (request.PageToken != null);

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

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                await client.UploadObjectAsync(bucketName, path.EnsureTrailingForwardSlash(), null, Stream.Null, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                var files = await GetFilesAsync(virtualPath, cancellationToken);

                foreach (var file in files)
                {
                    await DeleteFileAsync(file.VirtualPath, cancellationToken);
                }

                var subDirectories = await GetDirectoriesAsync(virtualPath, cancellationToken);

                foreach (var subDirectory in subDirectories)
                {
                    await DeleteDirectoryAsync(subDirectory.VirtualPath, cancellationToken);
                }

                await client.DeleteObjectAsync(bucketName, path, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                await client.DeleteObjectAsync(bucketName, path, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                var file = await client.GetObjectAsync(bucketName, path, new GetObjectOptions(), cancellationToken);

                var memoryStream = new MemoryStream();

                await client.DownloadObjectAsync(file, memoryStream, new DownloadObjectOptions(), cancellationToken);

                memoryStream.Position = 0;

                return memoryStream;
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

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                var file = new Object
                {
                    Bucket = bucketName,
                    Name = path,
                    ContentType = ContentTypeProvider.GetContentType(path)
                };

                await client.UploadObjectAsync(file, contents, new UploadObjectOptions(), cancellationToken);
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

            return new AdapterRuntimeException(exception);
        }
    }
}