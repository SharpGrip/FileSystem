using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.AzureBlobStorage
{
    public class AzureBlobStorageAdapter : Adapter<AzureBlobStorageAdapterConfiguration, string, string>
    {
        private readonly BlobContainerClient client;

        public AzureBlobStorageAdapter(string prefix, string rootPath, BlobContainerClient client, Action<AzureBlobStorageAdapterConfiguration>? configuration = null)
            : base(prefix, rootPath, configuration)
        {
            this.client = client;
        }

        public override void Dispose()
        {
        }

        public override void Connect()
        {
            Logger.LogStartConnectingAdapter(this);
            Logger.LogFinishedConnectingAdapter(this);
        }

        public override async Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);
            var directoryPath = GetParentPathPart(path);

            path = path.RemoveLeadingForwardSlash();

            try
            {
                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, directoryPath, cancellationToken))
                {
                    if (item.Name == path)
                    {
                        return ModelFactory.CreateFile(item, virtualPath);
                    }
                }

                throw new FileNotFoundException(path, Prefix);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);
            var directoryPath = GetLastPathPart(path);
            var parentDirectoryPath = GetParentPathPart(path);

            path = path.RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                if (path == "/")
                {
                    return ModelFactory.CreateDirectory("", path, virtualPath);
                }

                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, parentDirectoryPath, cancellationToken))
                {
                    if (item.Name.StartsWith(path))
                    {
                        return ModelFactory.CreateDirectory(directoryPath, path, virtualPath);
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

            path = path.RemoveLeadingForwardSlash();
            path = path.RemoveTrailingForwardSlash();

            try
            {
                var files = new List<IFile>();

                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, path, cancellationToken))
                {
                    var directoryPath = GetParentPathPart(item.Name);

                    if (directoryPath == path && item.Name != directoryPath + "/" && !item.Name.Contains(Configuration.DirectoryPlaceholder))
                    {
                        files.Add(ModelFactory.CreateFile(item, GetVirtualPath(item.Name)));
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

            path = path.RemoveLeadingForwardSlash();
            path = path.EnsureTrailingForwardSlash();

            try
            {
                var directories = new List<IDirectory>();

                if (path == "/")
                {
                    path = "";
                }

                await foreach (var item in client.GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, "/", path, cancellationToken))
                {
                    if (item.IsPrefix)
                    {
                        directories.Add(ModelFactory.CreateDirectory(GetLastPathPart(item.Prefix), item.Prefix, GetVirtualPath(item.Prefix)));
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

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                await client.UploadBlobAsync(path + Configuration.DirectoryPlaceholder, Stream.Null, cancellationToken);
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

            path = path.RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, path, cancellationToken))
                {
                    await client.DeleteBlobAsync(item.Name, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
                }
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
                await client.DeleteBlobAsync(path, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
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
                return await client.GetBlobClient(path).OpenReadAsync(cancellationToken: cancellationToken);
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
                contents.Seek(0, SeekOrigin.Begin);

                await client.UploadBlobAsync(path, contents, cancellationToken);
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

            if (exception is RequestFailedException requestFailedException)
            {
                if (requestFailedException.ErrorCode == "AuthenticationFailed")
                {
                    return new ConnectionException(exception);
                }
            }

            return new AdapterRuntimeException(exception);
        }
    }
}