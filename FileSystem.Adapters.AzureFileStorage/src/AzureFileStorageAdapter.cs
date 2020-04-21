using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public override IFile GetFile(string path)
        {
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var file = directory.GetFileClient(filePath);

                if (!file.Exists())
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

        public override IDirectory GetDirectory(string path)
        {
            path = PrependRootPath(path);

            try
            {
                var directory = client.GetDirectoryClient(path);

                if (!directory.Exists())
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

        public override IEnumerable<IFile> GetFiles(string path = "")
        {
            GetDirectory(path);
            path = PrependRootPath(path);

            try
            {
                var directory = client.GetDirectoryClient(path);
                var result = directory.GetFilesAndDirectories();

                return result.Where(item => !item.IsDirectory)
                    .Select(item => ModelFactory.CreateFile(directory.GetFileClient(item.Name)))
                    .Cast<IFile>()
                    .ToList();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            GetDirectory(path);
            path = PrependRootPath(path);

            try
            {
                var directory = client.GetDirectoryClient(path);
                var result = directory.GetFilesAndDirectories();

                return result.Where(item => item.IsDirectory)
                    .Select(item => ModelFactory.CreateDirectory(directory.GetSubdirectoryClient(item.Name)))
                    .Cast<IDirectory>()
                    .ToList();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override void CreateDirectory(string path)
        {
            try
            {
                client.CreateDirectory(PrependRootPath(path));
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override void DeleteFile(string path)
        {
            GetFile(path);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                directory.GetFileClient(filePath).Delete();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override void DeleteDirectory(string path)
        {
            GetDirectory(path);

            try
            {
                client.DeleteDirectory(PrependRootPath(path));
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<byte[]> ReadFileAsync(string path)
        {
            GetFile(path);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var download = await directory.GetFileClient(filePath).DownloadAsync();

                await using var memoryStream = new MemoryStream();
                await download.Value.Content.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<string> ReadTextFileAsync(string path)
        {
            GetFile(path);
            path = PrependRootPath(path);
            var pathParts = path.Split('/');
            var filePath = pathParts.Last();
            var directoryPath = string.Join('/', pathParts.Take(pathParts.Length - 1));

            try
            {
                var directory = client.GetDirectoryClient(directoryPath);
                var file = directory.GetFileClient(filePath);
                var download = await file.DownloadAsync();

                await using var memoryStream = new MemoryStream();
                await download.Value.Content.CopyToAsync(memoryStream);
                using var streamReader = new StreamReader(memoryStream);
                memoryStream.Position = 0;

                return await streamReader.ReadToEndAsync();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task WriteFileAsync(string path, byte[] contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
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
                await directory.CreateIfNotExistsAsync();
                var file = directory.GetFileClient(filePath);

                await using var memoryStream = new MemoryStream(contents);
                await file.CreateAsync(memoryStream.Length);
                await file.UploadRangeAsync(new HttpRange(0, memoryStream.Length), memoryStream);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task AppendFileAsync(string path, byte[] contents)
        {
            GetFile(path);
            var existingContents = await ReadFileAsync(path);
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

                await file.DeleteAsync();
                await file.CreateAsync(memoryStream.Length);

                await file.UploadRangeAsync(new HttpRange(0, memoryStream.Length), memoryStream);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }
    }
}