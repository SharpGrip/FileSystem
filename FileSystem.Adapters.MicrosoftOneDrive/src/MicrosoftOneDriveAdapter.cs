using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.MicrosoftOneDrive
{
    public class MicrosoftOneDriveAdapter : Adapter
    {
        private readonly GraphServiceClient client;
        private readonly string driveId;

        public MicrosoftOneDriveAdapter(string prefix, string rootPath, GraphServiceClient client, string driveId) : base(prefix, rootPath)
        {
            this.client = client;
            this.driveId = driveId;
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

            // Ensure that the path does not end with a '/'.
            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            try
            {
                var item = await GetItemAsync(path, cancellationToken);

                if (item.Folder != null)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(item, path);
            }
            catch (ServiceException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
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

            // Ensure that the path does not end with a '/'.
            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            try
            {
                var item = await GetItemAsync(path, cancellationToken);

                if (item.Folder == null)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(item, path);
            }
            catch (ServiceException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                throw Exception(exception);
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

            // Ensure that the path does not end with a '/'.
            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            try
            {
                var items = await GetItemsAsync(path, cancellationToken);
                var files = new List<IFile>();

                foreach (var item in items)
                {
                    if (item.Folder == null)
                    {
                        files.Add(ModelFactory.CreateFile(item, $"{path}/{item.Name}"));
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

            // Ensure that the path does not end with a '/'.
            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            try
            {
                var items = await GetItemsAsync(path, cancellationToken);
                var directories = new List<IDirectory>();

                foreach (var item in items)
                {
                    if (item.Folder != null)
                    {
                        directories.Add(ModelFactory.CreateDirectory(item, $"{path}/{item.Name}"));
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

            var directoryPath = GetLastPathPart(path);
            var parentPath = GetParentPathPart(path);
            var parentItem = await GetParentItemAsync(parentPath, cancellationToken);

            try
            {
                var item = new DriveItem
                {
                    Name = directoryPath,
                    Folder = new Folder()
                };

                await client.Drives[driveId].Items[parentItem.Id].Children.Request().AddAsync(item, cancellationToken);
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

            try
            {
                var item = await GetItemAsync(path, cancellationToken);

                await client.Drives[driveId].Items[item.Id].Request().DeleteAsync(cancellationToken);
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
                var item = await GetItemAsync(path, cancellationToken);

                await client.Drives[driveId].Items[item.Id].Request().DeleteAsync(cancellationToken);
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
                var item = await GetItemAsync(path, cancellationToken);

                var stream = await client.Drives[driveId].Items[item.Id].Content.Request().GetAsync(cancellationToken);
                await stream.CopyToAsync(memoryStream, cancellationToken);
                return memoryStream.ToArray();
#else
                using (var memoryStream = new MemoryStream())
                {
                    var item = await GetItemAsync(path, cancellationToken);
                    var stream = await client.Drives[driveId].Items[item.Id].Content.Request().GetAsync(cancellationToken);
                    await stream.CopyToAsync(memoryStream, 81920, cancellationToken);
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
                var item = await GetItemAsync(path, cancellationToken);

                var stream = await client.Drives[driveId].Items[item.Id].Content.Request().GetAsync(cancellationToken);
                await stream.CopyToAsync(memoryStream, cancellationToken);

                using var streamReader = new StreamReader(memoryStream);
                memoryStream.Position = 0;

                return await streamReader.ReadToEndAsync();
#else
                using (var memoryStream = new MemoryStream())
                {
                    var item = await GetItemAsync(path, cancellationToken);

                    var stream = await client.Drives[driveId].Items[item.Id].Content.Request().GetAsync(cancellationToken);
                    await stream.CopyToAsync(memoryStream, 81920, cancellationToken);

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
                var uploadSession = await client.Drives[driveId].Root.ItemWithPath(path).CreateUploadSession().Request()
                    .PostAsync(cancellationToken);
                var provider = new ChunkedUploadProvider(uploadSession, client, memoryStream);
                var chunkRequests = provider.GetUploadChunkRequests();
                var exceptionTrackingList = new List<Exception>();

                foreach (var request in chunkRequests)
                {
                    var result = await provider.GetChunkRequestResponseAsync(request, exceptionTrackingList);

                    if (!result.UploadSucceeded && exceptionTrackingList.Any())
                    {
                        throw new AdapterRuntimeException(exceptionTrackingList.First());
                    }
                }
#else
                using (var memoryStream = new MemoryStream(contents))
                {
                    var uploadSession = await client.Drives[driveId].Root.ItemWithPath(path).CreateUploadSession().Request()
                        .PostAsync(cancellationToken);
                    var provider = new ChunkedUploadProvider(uploadSession, client, memoryStream);
                    var chunkRequests = provider.GetUploadChunkRequests();
                    var exceptionTrackingList = new List<Exception>();

                    foreach (var request in chunkRequests)
                    {
                        var result = await provider.GetChunkRequestResponseAsync(request, exceptionTrackingList);

                        if (!result.UploadSucceeded && exceptionTrackingList.Any())
                        {
                            throw new AdapterRuntimeException(exceptionTrackingList.First());
                        }
                    }
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
                var uploadSession = await client.Drives[driveId].Root.ItemWithPath(path).CreateUploadSession().Request()
                    .PostAsync(cancellationToken);
                var provider = new ChunkedUploadProvider(uploadSession, client, memoryStream);

                var chunkRequests = provider.GetUploadChunkRequests();
                var exceptionTrackingList = new List<Exception>();

                foreach (var request in chunkRequests)
                {
                    var result = await provider.GetChunkRequestResponseAsync(request, exceptionTrackingList);

                    if (!result.UploadSucceeded && exceptionTrackingList.Any())
                    {
                        throw new AdapterRuntimeException(exceptionTrackingList.First());
                    }
                }
#else
                using (var memoryStream = new MemoryStream(contents))
                {
                    var uploadSession = await client.Drives[driveId].Root.ItemWithPath(path).CreateUploadSession().Request()
                        .PostAsync(cancellationToken);
                    var provider = new ChunkedUploadProvider(uploadSession, client, memoryStream);

                    var chunkRequests = provider.GetUploadChunkRequests();
                    var exceptionTrackingList = new List<Exception>();

                    foreach (var request in chunkRequests)
                    {
                        var result = await provider.GetChunkRequestResponseAsync(request, exceptionTrackingList);

                        if (!result.UploadSucceeded && exceptionTrackingList.Any())
                        {
                            throw new AdapterRuntimeException(exceptionTrackingList.First());
                        }
                    }
                }
#endif
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        private async Task<DriveItem> GetItemAsync(string path, CancellationToken cancellationToken = default)
        {
            DriveItem item;

            // The requested path is the root path, requesting the root item in the root context is not possible. Return the root instead.
            if (path == "")
            {
                item = await client.Drives[driveId].Root.Request().GetAsync(cancellationToken);
            }
            else
            {
                item = await client.Drives[driveId].Root.ItemWithPath(path).Request().GetAsync(cancellationToken);
            }

            return item;
        }

        private async Task<DriveItem> GetParentItemAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetItemAsync(path, cancellationToken);
            }
            catch (ServiceException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                throw Exception(exception);
            }
        }

        private async Task<IDriveItemChildrenCollectionPage> GetItemsAsync(string path, CancellationToken cancellationToken = default)
        {
            IDriveItemChildrenCollectionPage items;

            // The requested path is the root path, requesting the root item in the root context is not possible. Return all the children of the root instead.
            if (path == "")
            {
                items = await client.Drives[driveId].Root.Children.Request().GetAsync(cancellationToken);
            }
            else
            {
                items = await client.Drives[driveId].Root.ItemWithPath(path).Children.Request().GetAsync(cancellationToken);
            }

            return items;
        }

        private static Exception Exception(Exception exception)
        {
            if (exception is FileSystemException)
            {
                return exception;
            }

            if (exception is ServiceException serviceException)
            {
                if (serviceException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ConnectionException(exception);
                }

                if (serviceException.StatusCode == HttpStatusCode.Forbidden)
                {
                    return new ConnectionException(exception);
                }
            }

            return new AdapterRuntimeException(exception);
        }
    }
}