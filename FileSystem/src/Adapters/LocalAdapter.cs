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
    public class LocalAdapter : Adapter<LocalAdapterConfiguration, string, string>
    {
        public LocalAdapter(string prefix, string rootPath, Action<LocalAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
        }

        public override void Dispose()
        {
        }

        public override void Connect()
        {
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
            catch (FileSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
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
            catch (FileSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);
            var directory = await Task.Run(() => new DirectoryInfo(path), cancellationToken);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            try
            {
                return await Task.Run(() => directory.GetFiles().Select(item => GetFile(GetVirtualPath(item.FullName))).ToList(), cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);
            var directory = await Task.Run(() => new DirectoryInfo(path), cancellationToken);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            try
            {
                return await Task.Run(() => directory.GetDirectories().Select(item => GetDirectory(GetVirtualPath(item.FullName))).ToList(), cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
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
                throw new AdapterRuntimeException(exception);
            }
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);
            await Task.Run(() => File.Delete(GetPath(virtualPath)), cancellationToken);
        }

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            await Task.Run(() => Directory.Delete(GetPath(virtualPath), true), cancellationToken);
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            return new FileStream(GetPath(virtualPath), FileMode.Open);
        }

        public override async Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!overwrite && await FileExistsAsync(virtualPath, cancellationToken))
            {
                throw new FileExistsException(GetPath(virtualPath), Prefix);
            }

            using var fileStream = new FileStream(GetPath(virtualPath), FileMode.Create);
            contents.Seek(0, SeekOrigin.Begin);

            await contents.CopyToAsync(fileStream);
        }

        public new async Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            using var fileStream = new FileStream(GetPath(virtualPath), FileMode.Append);
            contents.Seek(0, SeekOrigin.Begin);

            await contents.CopyToAsync(fileStream);
        }

        protected override Exception Exception(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}