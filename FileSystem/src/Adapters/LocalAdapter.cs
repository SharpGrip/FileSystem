using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters
{
    public class LocalAdapter : Adapter
    {
        public LocalAdapter(string prefix, string rootPath) : base(prefix, rootPath)
        {
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

            try
            {
                var file = await Task.Run(() => new FileInfo(path), cancellationToken);

                if (!file.Exists)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return new FileModel(file);
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
                var directory = await Task.Run(() => new DirectoryInfo(path), cancellationToken);

                if (!directory.Exists)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return new DirectoryModel(directory);
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
            path = PrependRootPath(path);
            var directory = await Task.Run(() => new DirectoryInfo(path), cancellationToken);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            try
            {
                return await Task.Run(() => directory.GetFiles().Select(item => GetFile(item.FullName)).ToList(), cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(
            string path = "",
            CancellationToken cancellationToken = default
        )
        {
            path = PrependRootPath(path);
            var directory = await Task.Run(() => new DirectoryInfo(path), cancellationToken);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            try
            {
                return await Task.Run(
                    () => directory.GetDirectories().Select(item => GetDirectory(item.FullName)).ToList(),
                    cancellationToken
                );
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            if (await DirectoryExistsAsync(path, cancellationToken))
            {
                throw new DirectoryExistsException(PrependRootPath(path), Prefix);
            }

            try
            {
                await Task.Run(() => Directory.CreateDirectory(PrependRootPath(path)), cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            await Task.Run(() => File.Delete(PrependRootPath(path)), cancellationToken);
        }

        public override async Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            await Task.Run(() => Directory.Delete(PrependRootPath(path), true), cancellationToken);
        }

        public override async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);

            using var fileStream = new FileStream(PrependRootPath(path), FileMode.Open);
            var fileContents = new byte[fileStream.Length];

            await fileStream.ReadAsync(fileContents, 0, (int) fileStream.Length, cancellationToken);

            return fileContents;
        }

        public override async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);
            using var streamReader = new StreamReader(PrependRootPath(path));

            return await streamReader.ReadToEndAsync();
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

            using var fileStream = new FileStream(PrependRootPath(path), FileMode.Create);

            await fileStream.WriteAsync(contents, 0, contents.Length, cancellationToken);
        }

        public override async Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);

            using var fileStream = new FileStream(PrependRootPath(path), FileMode.Append);

            await fileStream.WriteAsync(contents, 0, contents.Length, cancellationToken);
        }
    }
}