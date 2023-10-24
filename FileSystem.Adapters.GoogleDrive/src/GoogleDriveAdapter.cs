using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using SharpGrip.FileSystem.Cache;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using File = Google.Apis.Drive.v3.Data.File;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.GoogleDrive
{
    public class GoogleDriveAdapter : Adapter<GoogleDriveAdapterConfiguration, string, File>
    {
        private readonly DriveService client;

        private const string DirectoryMimeType = "application/vnd.google-apps.folder";
        private const string SingleRequestFields = "id, name, size, modifiedTime, createdTime, parents";
        private static readonly string MultipleRequestFields = $"nextPageToken, files({SingleRequestFields})";
        private static readonly string FileRequestQuery = $"mimeType != '{DirectoryMimeType}' and trashed = false";
        private static readonly string DirectoryRequestQuery = $"mimeType = '{DirectoryMimeType}' and trashed = false";

        public GoogleDriveAdapter(string prefix, string rootPath, DriveService client, Action<GoogleDriveAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
            this.client = client;
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
            var path = GetPath(virtualPath).RemoveLeadingForwardSlash();

            try
            {
                var file = await RequestFileByPath(path, cancellationToken);

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
            var path = GetPath(virtualPath).EnsureTrailingForwardSlash();

            try
            {
                if (path == "/")
                {
                    return ModelFactory.CreateDirectory(new File {Name = "/"}, path, virtualPath);
                }

                path = path.RemoveLeadingForwardSlash();

                var directory = await RequestDirectoryByPath(path, cancellationToken);

                if (directory == null)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory, path, virtualPath);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).EnsureTrailingForwardSlash().RemoveLeadingForwardSlash();

            if (path == "")
            {
                path = "/";
            }

            try
            {
                var request = client.Files.List();
                FileList fileList;

                request.Q = FileRequestQuery;
                request.Fields = MultipleRequestFields;

                var files = new List<IFile>();

                do
                {
                    fileList = await request.ExecuteAsync(cancellationToken);

                    foreach (var file in fileList.Files)
                    {
                        TryAddCacheEntry(new CacheEntry<string, File>(file.Id, file));

                        var filePath = await GetAbsolutePath(file);
                        var directoryPath = GetParentPathPart(filePath).EnsureTrailingForwardSlash();

                        if (directoryPath == path)
                        {
                            files.Add(ModelFactory.CreateFile(file, filePath, GetVirtualPath(filePath)));
                        }
                    }

                    request.PageToken = fileList.NextPageToken;
                } while (fileList.NextPageToken != null);

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

            var path = GetPath(virtualPath).EnsureTrailingForwardSlash().RemoveLeadingForwardSlash();

            if (path == "")
            {
                path = "/";
            }

            try
            {
                var request = client.Files.List();
                FileList directoryList;

                request.Q = DirectoryRequestQuery;
                request.Fields = MultipleRequestFields;

                var directories = new List<IDirectory>();

                do
                {
                    directoryList = await request.ExecuteAsync(cancellationToken);

                    foreach (var directory in directoryList.Files)
                    {
                        TryAddCacheEntry(new CacheEntry<string, File>(directory.Id, directory));

                        var directoryPath = (await GetAbsolutePath(directory)).EnsureTrailingForwardSlash();
                        var directoryParentPathPart = GetParentPathPart(directoryPath).EnsureTrailingForwardSlash();

                        if (directoryParentPathPart == path)
                        {
                            directories.Add(ModelFactory.CreateDirectory(directory, directoryPath, GetVirtualPath(directoryPath)));
                        }
                    }

                    request.PageToken = directoryList.NextPageToken;
                } while (directoryList.NextPageToken != null);

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

            var path = GetPath(virtualPath).RemoveTrailingForwardSlash().RemoveLeadingForwardSlash();

            if (path == "")
            {
                path = "/";
            }

            try
            {
                var parent = await RequestParentDirectory(path, cancellationToken);

                var directory = new File
                {
                    Name = GetLastPathPart(path),
                    Parents = new List<string> {parent.Id},
                    MimeType = DirectoryMimeType
                };

                var request = client.Files.Create(directory);
                request.Fields = SingleRequestFields;

                var response = await request.ExecuteAsync(cancellationToken);

                TryAddCacheEntry(new CacheEntry<string, File>(response.Id, response));
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
                var directory = await RequestDirectoryByPath(path, cancellationToken);

                if (directory == null)
                {
                    throw new DirectoryNotFoundException(GetPath(virtualPath), Prefix);
                }

                var deleteRequest = client.Files.Delete(directory.Id);
                await deleteRequest.ExecuteAsync(cancellationToken);

                TryRemoveCacheEntry(directory.Id);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash().EnsureTrailingForwardSlash();

            try
            {
                var file = await RequestFileByPath(path, cancellationToken);

                if (file == null)
                {
                    throw new FileNotFoundException(GetPath(virtualPath), Prefix);
                }

                var deleteRequest = client.Files.Delete(file.Id);
                await deleteRequest.ExecuteAsync(cancellationToken);

                TryRemoveCacheEntry(file.Id);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).RemoveLeadingForwardSlash();

            try
            {
                var file = await RequestFileByPath(path, cancellationToken);

                var memoryStream = new MemoryStream();
                var request = client.Files.Get(file!.Id);

                await request.DownloadAsync(memoryStream, cancellationToken);

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

            var path = GetPath(virtualPath);

            try
            {
                var parent = await RequestParentDirectory(path, cancellationToken);
                var contentType = ContentTypeProvider.GetContentType(path);

                var file = new File
                {
                    Name = GetLastPathPart(path),
                    Parents = new List<string> {parent.Id},
                    MimeType = contentType
                };

                var request = client.Files.Create(file, contents, contentType);
                request.Fields = SingleRequestFields;

                await request.UploadAsync(cancellationToken);

                var response = request.ResponseBody;

                TryAddCacheEntry(new CacheEntry<string, File>(response.Id, response));
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

        private async Task<string> GetAbsolutePath(File file)
        {
            if (file.Parents == null || !file.Parents.Any())
            {
                return file.Name;
            }

            var pathParts = new List<string> {file.Name};

            while (file.Parents != null && file.Parents.Any())
            {
                var parentId = file.Parents[0];
                var cacheEntry = await GetOrCreateCacheEntryAsync(parentId, async () => new CacheEntry<string, File>(parentId, await RequestFileById(parentId)));
                var parent = cacheEntry.Value;

                if (parent.Parents == null || !parent.Parents.Any())
                {
                    break;
                }

                pathParts.Insert(0, parent.Name);
                file = parent;
            }

            return string.Join("/", pathParts);
        }

        private async Task<File> RequestParentDirectory(string path, CancellationToken cancellationToken = default)
        {
            var parentPathPart = GetParentPathPart(path);

            var parentDirectory = await RequestDirectoryByPath(parentPathPart, cancellationToken);

            if (parentDirectory != null)
            {
                return parentDirectory;
            }

            return await RequestRootDrive(cancellationToken);
        }

        private async Task<File> RequestRootDrive(CancellationToken cancellationToken = default)
        {
            return await RequestFileById("root", cancellationToken);
        }

        private async Task<File> RequestFileById(string id, CancellationToken cancellationToken = default)
        {
            var request = client.Files.Get(id);
            request.Fields = SingleRequestFields;

            var file = await request.ExecuteAsync(cancellationToken);

            TryAddCacheEntry(new CacheEntry<string, File>(file.Id, file));

            return file;
        }

        private async Task<File?> RequestFileByPath(string path, CancellationToken cancellationToken = default)
        {
            var request = client.Files.List();

            request.Q = $"mimeType != '{DirectoryMimeType}' and name = '{GetLastPathPart(path)}' and trashed = false";
            request.Fields = MultipleRequestFields;

            do
            {
                var fileList = await request.ExecuteAsync(cancellationToken);

                foreach (var file in fileList.Files)
                {
                    TryAddCacheEntry(new CacheEntry<string, File>(file.Id, file));

                    var filePath = await GetAbsolutePath(file);

                    if (filePath == path)
                    {
                        return file;
                    }
                }

                request.PageToken = fileList.NextPageToken;
            } while (request.PageToken != null);

            return null;
        }

        private async Task<File?> RequestDirectoryByPath(string path, CancellationToken cancellationToken = default)
        {
            var request = client.Files.List();
            FileList directoryList;

            request.Q = $"mimeType = '{DirectoryMimeType}' and name = '{GetLastPathPart(path)}' and trashed = false";
            request.Fields = MultipleRequestFields;

            do
            {
                directoryList = await request.ExecuteAsync(cancellationToken);

                foreach (var directory in directoryList.Files)
                {
                    TryAddCacheEntry(new CacheEntry<string, File>(directory.Id, directory));

                    var directoryPath = (await GetAbsolutePath(directory)).EnsureTrailingForwardSlash();

                    if (directoryPath == path)
                    {
                        return directory;
                    }
                }

                request.PageToken = directoryList.NextPageToken;
            } while (directoryList.NextPageToken != null);

            return null;
        }
    }
}