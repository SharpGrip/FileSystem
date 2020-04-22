using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.AzureFileStorage
{
    public class AzureFileStorageAdapter : Adapter
    {
        private readonly ShareClient client;

        public AzureFileStorageAdapter(string prefix, string rootPath, ShareClient client) : base(prefix, rootPath)
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
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var file = directory.GetFileClient(filePath);

                if (!await file.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file);
            }
            catch (FileSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);

            try
            {
                var directory = client.GetDirectoryClient(path);

                if (!await directory.ExistsAsync(cancellationToken))
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory);
            }
            catch (FileSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string path = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                var directory = client.GetDirectoryClient(path);
                var enumerator = directory.GetFilesAndDirectoriesAsync().GetAsyncEnumerator(cancellationToken);

                var files = new List<IFile>();

                while (await enumerator.MoveNextAsync())
                {
                    if (!enumerator.Current.IsDirectory)
                    {
                        files.Add(ModelFactory.CreateFile(directory.GetFileClient(enumerator.Current.Name)));
                    }
                }

                return files;
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string path = "",
            CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                var directory = client.GetDirectoryClient(path);
                var enumerator = directory.GetFilesAndDirectoriesAsync().GetAsyncEnumerator(cancellationToken);

                var directories = new List<IDirectory>();

                while (await enumerator.MoveNextAsync())
                {
                    if (enumerator.Current.IsDirectory)
                    {
                        directories.Add(ModelFactory.CreateDirectory(directory.GetSubdirectoryClient(enumerator.Current.Name)));
                    }
                }

                return directories;
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.CreateDirectoryAsync(PrependRootPath(path), cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                await directory.GetFileClient(filePath).DeleteAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);

            try
            {
                await client.DeleteDirectoryAsync(PrependRootPath(path), cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var download = await directory.GetFileClient(filePath).DownloadAsync(cancellationToken: cancellationToken);

                await using var memoryStream = new MemoryStream();
                await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var file = directory.GetFileClient(filePath);
                var download = await file.DownloadAsync(cancellationToken: cancellationToken);

                await using var memoryStream = new MemoryStream();
                await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
                using var streamReader = new StreamReader(memoryStream);
                memoryStream.Position = 0;

                return await streamReader.ReadToEndAsync();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
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
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                await directory.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                var file = directory.GetFileClient(filePath);

                await using var memoryStream = new MemoryStream(contents);
                await file.CreateAsync(memoryStream.Length, cancellationToken: cancellationToken);
                await file.UploadRangeAsync(new HttpRange(0, memoryStream.Length), memoryStream, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            var existingContents = await ReadFileAsync(path, cancellationToken);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var file = directory.GetFileClient(filePath);

                contents = existingContents.Concat(contents).ToArray();
                await using var memoryStream = new MemoryStream(contents);

                await file.DeleteAsync(cancellationToken);
                await file.CreateAsync(memoryStream.Length, cancellationToken: cancellationToken);

                await file.UploadRangeAsync(new HttpRange(0, memoryStream.Length), memoryStream, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }
    }
}