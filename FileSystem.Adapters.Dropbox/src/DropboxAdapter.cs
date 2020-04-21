using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public override IFile GetFile(string path)
        {
            path = PrependRootPath(path);

            try
            {
                var file = client.Files.GetMetadataAsync(path).Result;

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
            // @todo find out if there is a better way to detect if the path is not found
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

        public override IDirectory GetDirectory(string path)
        {
            path = PrependRootPath(path);

            try
            {
                var directory = client.Files.GetMetadataAsync(path).Result;

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
            // @todo find out if there is a better way to detect if the path is not found
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

        public override IEnumerable<IFile> GetFiles(string path = "")
        {
            GetDirectory(path);
            path = PrependRootPath(path);

            try
            {
                var result = client.Files.ListFolderAsync(path).Result;

                return result.Entries.Where(item => !item.IsFolder).Select(ModelFactory.CreateFile).ToList();
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
                var result = client.Files.ListFolderAsync(path).Result;

                return result.Entries.Where(item => item.IsFolder).Select(ModelFactory.CreateDirectory).ToList();
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
                client.Files.CreateFolderV2Async(PrependRootPath(path)).Wait();
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override void DeleteFile(string path)
        {
            GetFile(path);
            client.Files.DeleteV2Async(PrependRootPath(path)).Wait();
        }

        public override void DeleteDirectory(string path)
        {
            GetDirectory(path);
            client.Files.DeleteV2Async(PrependRootPath(path)).Wait();
        }

        public override async Task<byte[]> ReadFileAsync(string path)
        {
            GetFile(path);
            using var response = await client.Files.DownloadAsync(PrependRootPath(path));

            return await response.GetContentAsByteArrayAsync();
        }

        public override async Task<string> ReadTextFileAsync(string path)
        {
            GetFile(path);
            using var response = await client.Files.DownloadAsync(PrependRootPath(path));

            return await response.GetContentAsStringAsync();
        }

        public override async Task WriteFileAsync(string path, byte[] contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await using var memoryStream = new MemoryStream(contents);
            await client.Files.UploadAsync(PrependRootPath(path), WriteMode.Overwrite.Instance, body: memoryStream);
        }

        public override async Task AppendFileAsync(string path, byte[] contents)
        {
            GetFile(path);
            var existingContents = await ReadFileAsync(path);
            contents = existingContents.Concat(contents).ToArray();

            await using var memoryStream = new MemoryStream(contents);
            await client.Files.UploadAsync(PrependRootPath(path), WriteMode.Overwrite.Instance, body: memoryStream);
        }
    }
}