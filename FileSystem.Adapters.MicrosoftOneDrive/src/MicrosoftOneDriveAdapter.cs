using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.MicrosoftOneDrive
{
    public class MicrosoftOneDriveAdapter : Adapter<MicrosoftOneDriveAdapterConfiguration, string, string>
    {
        private readonly GraphServiceClient client;
        private readonly string driveId;

        public MicrosoftOneDriveAdapter(string prefix, string rootPath, GraphServiceClient client, string driveId, Action<MicrosoftOneDriveAdapterConfiguration>? configuration = null)
            : base(prefix, rootPath, configuration)
        {
            this.client = client;
            this.driveId = driveId;
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

                return ModelFactory.CreateFile(item, path, virtualPath);
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

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);

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

                return ModelFactory.CreateDirectory(item, path, virtualPath);
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

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

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
                        var filePath = $"{path}/{item.Name}";

                        files.Add(ModelFactory.CreateFile(item, filePath, GetVirtualPath(filePath)));
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
                        var directoryPath = $"{path}/{item.Name}";

                        directories.Add(ModelFactory.CreateDirectory(item, directoryPath, GetVirtualPath(directoryPath)));
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

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

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

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

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

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            try
            {
                var item = await GetItemAsync(path, cancellationToken);

                return await client.Drives[driveId].Items[item.Id].Content.Request().GetAsync(cancellationToken);
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
                var uploadSession = await client.Drives[driveId].Root.ItemWithPath(path).CreateUploadSession().Request().PostAsync(cancellationToken);
                var largeFileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, contents);

                var result = await largeFileUploadTask.UploadAsync();

                if (!result.UploadSucceeded)
                {
                    throw new AdapterRuntimeException();
                }
            }
            catch (TaskCanceledException exception) when (exception.InnerException != null)
            {
                throw Exception(exception.InnerException);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        private async Task<DriveItem> GetItemAsync(string path, CancellationToken cancellationToken = default)
        {
            // The requested path is the root path, requesting the root item in the root context is not possible. Return the root instead.
            if (path == "")
            {
                return await client.Drives[driveId].Root.Request().GetAsync(cancellationToken);
            }

            return await client.Drives[driveId].Root.ItemWithPath(path).Request().GetAsync(cancellationToken);
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
            // The requested path is the root path, requesting the root item in the root context is not possible. Return all the children of the root instead.
            if (path == "")
            {
                return await client.Drives[driveId].Root.Children.Request().GetAsync(cancellationToken);
            }

            return await client.Drives[driveId].Root.ItemWithPath(path).Children.Request().GetAsync(cancellationToken);
        }

        protected override Exception Exception(Exception exception)
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