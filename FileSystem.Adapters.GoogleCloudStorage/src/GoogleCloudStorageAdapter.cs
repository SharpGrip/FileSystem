using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

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
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().EnsureTrailingForwardSlash();
            var parentPath = GetParentPathPart(path).EnsureTrailingForwardSlash();

            try
            {
                var request = client.Service.Objects.List(bucketName);

                request.Prefix = parentPath == "/" ? null : parentPath;
                request.Delimiter = "/";

                do
                {
                    var objects = await request.ExecuteAsync(cancellationToken: cancellationToken);

                    foreach (var directoryName in objects.Prefixes)
                    {
                        var directoryPath = parentPath + directoryName;

                        if (directoryPath == path)
                        {
                            return ModelFactory.CreateDirectory(directoryName.RemoveTrailingForwardSlash(), directoryPath, GetVirtualPath(directoryPath));
                        }
                    }

                    request.PageToken = objects.NextPageToken;
                } while (request.PageToken != null);

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

                    foreach (var file in objects.Items)
                    {
                        var filePath = path + file.Name;

                        files.Add(ModelFactory.CreateFile(file, filePath.RemoveTrailingForwardSlash(), GetVirtualPath(filePath)));
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

                    foreach (var directoryName in objects.Prefixes)
                    {
                        var directoryPath = path + directoryName;

                        directories.Add(ModelFactory.CreateDirectory(directoryName.RemoveTrailingForwardSlash(), directoryPath.EnsureTrailingForwardSlash(), GetVirtualPath(directoryPath)));
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
                // client.Service.
                
                await client.UploadObjectAsync(bucketName, GetLastPathPart(path).EnsureTrailingForwardSlash(), null, Stream.Null, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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