using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.Dropbox
{
    public class DropboxAdapter : Adapter
    {
        private readonly DropboxClient client;

        public DropboxAdapter(string prefix, string rootPath, DropboxClient client) : base(prefix, rootPath)
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

        public override async Task<IFile> GetFileAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);

            try
            {
                var file = await client.Files.GetMetadataAsync(path);

                if (file.IsFolder)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file);
            }
            catch (FileSystemException)
            {
                throw;
            }
            catch (AggregateException exception)
            {
                if (exception.Message.Contains("path/not_found"))
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                throw new AdapterRuntimeException(exception);
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
                var directory = await client.Files.GetMetadataAsync(path);

                if (directory.IsFile)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory);
            }
            catch (FileSystemException)
            {
                throw;
            }
            catch (AggregateException exception)
            {
                if (exception.Message.Contains("path/not_found"))
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                throw new AdapterRuntimeException(exception);
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
                var result = await client.Files.ListFolderAsync(path);

                return result.Entries.Where(item => !item.IsFolder).Select(ModelFactory.CreateFile).ToList();
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
                var result = await client.Files.ListFolderAsync(path);

                return result.Entries.Where(item => item.IsFolder).Select(ModelFactory.CreateDirectory).ToList();
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
                await client.Files.CreateFolderV2Async(PrependRootPath(path));
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            await client.Files.DeleteV2Async(PrependRootPath(path));
        }

        public override async Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            client.Files.DeleteV2Async(PrependRootPath(path)).Wait(cancellationToken);
        }

        public override async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            using var response = await client.Files.DownloadAsync(PrependRootPath(path));

            return await response.GetContentAsByteArrayAsync();
        }

        public override async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            using var response = await client.Files.DownloadAsync(PrependRootPath(path));

            return await response.GetContentAsStringAsync();
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

            await using var memoryStream = new MemoryStream(contents);
            await client.Files.UploadAsync(PrependRootPath(path), WriteMode.Overwrite.Instance, body: memoryStream);
        }

        public override async Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            var existingContents = await ReadFileAsync(path, cancellationToken);
            contents = existingContents.Concat(contents).ToArray();

            await using var memoryStream = new MemoryStream(contents);
            await client.Files.UploadAsync(PrependRootPath(path), WriteMode.Overwrite.Instance, body: memoryStream);
        }
    }
}