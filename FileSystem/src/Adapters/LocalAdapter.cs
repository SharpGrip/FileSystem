using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Constants;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters
{
    public class LocalAdapter : Adapter<LocalAdapterConfiguration, string, string>
    {
        public LocalAdapter(string prefix, string rootPath, Action<LocalAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
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

            try
            {
                var file = await Task.Run(() => new FileInfo(path), cancellationToken);

                if (!file.Exists)
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
                var directory = await Task.Run(() => new DirectoryInfo(path), cancellationToken);

                if (!directory.Exists)
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
            var path = GetPath(virtualPath);
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            try
            {
                var files = new List<IFile>();

                foreach (var file in directory.GetFiles())
                {
                    files.Add(await GetFileAsync(GetVirtualPath(file.FullName), cancellationToken));
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
            var path = GetPath(virtualPath);

            try
            {
                var directory = new DirectoryInfo(path);

                if (!directory.Exists)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                var directories = new List<IDirectory>();

                foreach (var subDirectory in directory.GetDirectories())
                {
                    directories.Add(await GetDirectoryAsync(GetVirtualPath(subDirectory.FullName), cancellationToken));
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
                await Task.Run(() => Directory.CreateDirectory(GetPath(virtualPath)), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                await Task.Run(() => File.Delete(GetPath(virtualPath)), cancellationToken);
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
                await Task.Run(() => Directory.Delete(GetPath(virtualPath), true), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                return new FileStream(GetPath(virtualPath), FileMode.Open);
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

            try
            {
                using var fileStream = new FileStream(GetPath(virtualPath), FileMode.Create);
                contents.Seek(0, SeekOrigin.Begin);

                await contents.CopyToAsync(fileStream, FileSystemConstants.Streaming.DefaultMemoryStreamBufferSize, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                using var fileStream = new FileStream(GetPath(virtualPath), FileMode.Append);
                contents.Seek(0, SeekOrigin.Begin);

                await contents.CopyToAsync(fileStream, FileSystemConstants.Streaming.DefaultMemoryStreamBufferSize, cancellationToken);
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
    }
}