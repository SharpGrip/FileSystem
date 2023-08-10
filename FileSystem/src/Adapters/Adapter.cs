using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Constants;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters
{
    public abstract class Adapter : IAdapter
    {
        public string Prefix { get; }
        public string RootPath { get; }

        protected Adapter(string prefix, string rootPath)
        {
            Prefix = prefix;
            RootPath = rootPath;
        }

        public IFile GetFile(string virtualPath)
        {
            return GetFileAsync(virtualPath).Result;
        }

        public IDirectory GetDirectory(string virtualPath)
        {
            return GetDirectoryAsync(virtualPath).Result;
        }

        public IEnumerable<IFile> GetFiles(string virtualPath = "")
        {
            return GetFilesAsync(virtualPath).Result;
        }

        public IEnumerable<IDirectory> GetDirectories(string virtualPath = "")
        {
            return GetDirectoriesAsync(virtualPath).Result;
        }

        public bool FileExists(string virtualPath)
        {
            return FileExistsAsync(virtualPath).Result;
        }

        public async Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            try
            {
                await GetFileAsync(virtualPath, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        public bool DirectoryExists(string virtualPath)
        {
            return DirectoryExistsAsync(virtualPath).Result;
        }

        public async Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            try
            {
                await GetDirectoryAsync(virtualPath, cancellationToken);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }

            return true;
        }

        public void CreateDirectory(string virtualPath)
        {
            CreateDirectoryAsync(virtualPath).Wait();
        }

        public void DeleteDirectory(string virtualPath)
        {
            DeleteDirectoryAsync(virtualPath).Wait();
        }

        public void DeleteFile(string virtualPath)
        {
            DeleteFileAsync(virtualPath).Wait();
        }

        public byte[] ReadFile(string virtualPath)
        {
            return ReadFileAsync(virtualPath).Result;
        }

        public async Task<byte[]> ReadFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                var stream = await ReadFileStreamAsync(virtualPath, cancellationToken);
                using var memoryStream = await StreamUtilities.CopyContentsToMemoryStreamAsync(stream, cancellationToken);

                await stream.CopyToAsync(memoryStream, AdapterConstants.DefaultMemoryStreamBufferSize, cancellationToken);

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public async Task<string> ReadTextFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                using var stream = await ReadFileStreamAsync(virtualPath, cancellationToken);
                using var streamReader = new StreamReader(stream);

                return await streamReader.ReadToEndAsync();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public string ReadTextFile(string virtualPath)
        {
            return ReadTextFileAsync(virtualPath).Result;
        }

        public void WriteFile(string virtualPath, byte[] contents, bool overwrite = false)
        {
            WriteFileAsync(virtualPath, contents, overwrite).Wait();
        }

        public async Task WriteFileAsync(string virtualPath, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            await WriteFileAsync(virtualPath, new MemoryStream(contents), overwrite, cancellationToken);
        }

        public void WriteFile(string virtualPath, string contents, bool overwrite = false)
        {
            WriteFileAsync(virtualPath, contents, overwrite).Wait();
        }

        public async Task WriteFileAsync(string virtualPath, string contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            await WriteFileAsync(virtualPath, Encoding.UTF8.GetBytes(contents), overwrite, cancellationToken);
        }

        public virtual async Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var memoryStream = await StreamUtilities.CopyContentsToMemoryStreamAsync(contents, cancellationToken);

            var existingContents = await ReadFileAsync(virtualPath, cancellationToken);
            var fileContents = existingContents.Concat(memoryStream.ToArray()).ToArray();

            await DeleteFileAsync(virtualPath, cancellationToken);

            try
            {
                await WriteFileAsync(virtualPath, fileContents, true, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public void AppendFile(string virtualPath, byte[] contents)
        {
            AppendFileAsync(virtualPath, contents).Wait();
        }

        public async Task AppendFileAsync(string virtualPath, byte[] contents, CancellationToken cancellationToken = default)
        {
            await AppendFileAsync(virtualPath, new MemoryStream(contents), cancellationToken);
        }

        public void AppendFile(string virtualPath, string contents)
        {
            AppendFileAsync(virtualPath, contents).Wait();
        }

        public async Task AppendFileAsync(string virtualPath, string contents, CancellationToken cancellationToken = default)
        {
            await AppendFileAsync(virtualPath, Encoding.UTF8.GetBytes(contents), cancellationToken);
        }

        public abstract void Dispose();
        public abstract void Connect();
        public abstract Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public abstract Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default);
        protected abstract Exception Exception(Exception exception);

        protected string GetPath(string path)
        {
            return PathUtilities.GetPath(path, RootPath);
        }

        protected string GetVirtualPath(string path)
        {
            return PathUtilities.GetVirtualPath(path, Prefix, RootPath);
        }

        protected string[] GetPathParts(string path)
        {
            return path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
        }

        protected string GetLastPathPart(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return "";
            }

            return GetPathParts(path).Last();
        }

        protected string GetParentPathPart(string path)
        {
            if (path.IsNullOrEmpty())
            {
                path = "/";
            }

            var pathParts = GetPathParts(path);

            return string.Join("/", pathParts.Take(pathParts.Length - 1));
        }
    }
}