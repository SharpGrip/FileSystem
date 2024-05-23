using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.AzureFileStorage
{
    public class AzureFileStorageAdapter : Adapter<AzureFileStorageAdapterConfiguration, string, string>
    {
        private readonly ShareClient client;

        public AzureFileStorageAdapter(string prefix, string rootPath, ShareClient client, Action<AzureFileStorageAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
            this.client = client;
        }

        public override void Dispose()
        {
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogStartConnectingAdapter(this);
            await Task.CompletedTask;
            Logger.LogFinishedConnectingAdapter(this);
        }

        public override async Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);
            var filePath = GetLastPathPart(path);
            var directoryPath = GetParentPathPart(path);

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var file = directory.GetFileClient(filePath);

                if (!await file.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file, virtualPath);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);

            try
            {
                var directory = client.GetDirectoryClient(path);

                if (!await directory.ExistsAsync(cancellationToken))
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory, virtualPath);
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

            try
            {
                var currentDirectory = client.GetDirectoryClient(path);
                await using var currentDirectoryEnumerator = currentDirectory.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken).GetAsyncEnumerator(cancellationToken);

                var files = new List<IFile>();

                while (await currentDirectoryEnumerator.MoveNextAsync())
                {
                    if (!currentDirectoryEnumerator.Current.IsDirectory)
                    {
                        var file = currentDirectory.GetFileClient(currentDirectoryEnumerator.Current.Name);

                        files.Add(ModelFactory.CreateFile(file, GetVirtualPath(file.Path)));
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
                var currentDirectory = client.GetDirectoryClient(path);
                await using var currentDirectoryEnumerator = currentDirectory.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken).GetAsyncEnumerator(cancellationToken);

                var directories = new List<IDirectory>();

                while (await currentDirectoryEnumerator.MoveNextAsync())
                {
                    if (currentDirectoryEnumerator.Current.IsDirectory)
                    {
                        var directory = currentDirectory.GetSubdirectoryClient(currentDirectoryEnumerator.Current.Name);

                        directories.Add(ModelFactory.CreateDirectory(directory, GetVirtualPath(directory.Path)));
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

            try
            {
                await client.CreateDirectoryAsync(GetPath(virtualPath), cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

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

                await client.DeleteDirectoryAsync(GetPath(virtualPath), cancellationToken);
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
            var filePath = GetLastPathPart(path);
            var directoryPath = GetParentPathPart(path);

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                await directory.GetFileClient(filePath).DeleteAsync(cancellationToken);
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
            var filePath = GetLastPathPart(path);
            var directoryPath = GetParentPathPart(path);

            try
            {
                // Performance issue:
                // The stream returned from the service does not support seeking and therefore cannot determine the content length (required when creating files from this stream).
                // Copy the response stream to a new memory stream and return that one instead.

                var directory = client.GetDirectoryClient(directoryPath);
                var response = await directory.GetFileClient(filePath).DownloadAsync(cancellationToken: cancellationToken);

                var memoryStream = await StreamUtilities.CopyContentsToMemoryStreamAsync(response.Value.Content, true, cancellationToken);

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

            var path = GetPath(virtualPath);
            var filePath = GetLastPathPart(path);
            var directoryPath = GetParentPathPart(path).EnsureTrailingForwardSlash();

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);

                if (directoryPath != "/")
                {
                    await directory.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                }

                var file = directory.GetFileClient(filePath);

                contents.Seek(0, SeekOrigin.Begin);

                await file.CreateAsync(contents.Length, cancellationToken: cancellationToken);
                await file.UploadRangeAsync(new HttpRange(0, contents.Length), contents, cancellationToken: cancellationToken);
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