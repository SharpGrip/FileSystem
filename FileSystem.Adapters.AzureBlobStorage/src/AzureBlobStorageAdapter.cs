using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.AzureBlobStorage
{
    public class AzureBlobStorageAdapter : Adapter
    {
        private readonly BlobContainerClient client;

        public AzureBlobStorageAdapter(string prefix, string rootPath, BlobContainerClient client) : base(prefix, rootPath)
        {
            this.client = client;
        }

        public override void Dispose()
        {
        }

        public override void Connect()
        {
        }

        public override async Task<IFile> GetFileAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);
            var directoryPath = GetParentPathPart(path);

            try
            {
                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, directoryPath))
                {
                    if (item.Name == path)
                    {
                        return ModelFactory.CreateFile(item);
                    }
                }

                throw new FileNotFoundException(path, Prefix);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);
            var directoryPath = GetLastPathPart(path);
            var parentDirectoryPath = GetLastPathPart(path);

            try
            {
                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, parentDirectoryPath))
                {
                    if (item.Name.StartsWith(path))
                    {
                        return ModelFactory.CreateDirectory(directoryPath, path);
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
                var files = new List<IFile>();

                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, path))
                {
                    var directoryPath = GetParentPathPart(path);

                    if (directoryPath == path && item.Name != directoryPath + "/")
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

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string path = "",
            CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                var directories = new List<IDirectory>();
                path = path.EndsWith("/") ? path : path + "/";

                await foreach (var item in client.GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, "/", path))
                {
                    if (item.IsPrefix)
                    {
                        directories.Add(ModelFactory.CreateDirectory(GetLastPathPart(item.Prefix), item.Prefix));
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
            path = path.EndsWith("/") ? path : path + "/";

            try
            {
                await client.UploadBlobAsync(path, Stream.Null, cancellationToken);
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
            path = path.EndsWith("/") ? path : path + "/";

            try
            {
                await foreach (var item in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, path))
                {
                    await client.DeleteBlobAsync(item.Name, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
                }
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
                await client.DeleteBlobAsync(path, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
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
#if NETSTANDARD2_1
                await using var memoryStream = new MemoryStream();
                await client.GetBlobClient(path).DownloadToAsync(memoryStream, cancellationToken);

                return memoryStream.ToArray();
#else
                using (var memoryStream = new MemoryStream())
                {
                    await client.GetBlobClient(path).DownloadToAsync(memoryStream, cancellationToken);
                    return memoryStream.ToArray();
                }
#endif
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
#if NETSTANDARD2_1
                await using var memoryStream = new MemoryStream();
                await client.GetBlobClient(path).DownloadToAsync(memoryStream, cancellationToken);

                using var streamReader = new StreamReader(memoryStream);
                memoryStream.Position = 0;

                return await streamReader.ReadToEndAsync();
#else
                using (var memoryStream = new MemoryStream())
                {
                    await client.GetBlobClient(path).DownloadToAsync(memoryStream, cancellationToken);

                    using var streamReader = new StreamReader(memoryStream);
                    memoryStream.Position = 0;

                    return await streamReader.ReadToEndAsync();
                }
#endif
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task WriteFileAsync(string path, byte[] contents, bool overwrite = false,
            CancellationToken cancellationToken = default)
        {
            if (!overwrite && await FileExistsAsync(path, cancellationToken))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            path = PrependRootPath(path);

            try
            {
#if NETSTANDARD2_1
                await using var memoryStream = new MemoryStream(contents);

                await client.UploadBlobAsync(path, memoryStream, cancellationToken);
#else
                using (var memoryStream = new MemoryStream(contents))
                {
                    await client.UploadBlobAsync(path, memoryStream, cancellationToken);
                }
#endif
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
#if NETSTANDARD2_1
                await using var memoryStream = new MemoryStream(contents);

                await client.UploadBlobAsync(path, memoryStream, cancellationToken);
#else
                using (var memoryStream = new MemoryStream(contents))
                {
                    await client.UploadBlobAsync(path, memoryStream, cancellationToken);
                }
#endif
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