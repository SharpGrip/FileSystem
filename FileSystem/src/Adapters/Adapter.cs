using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;

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

        public IFile GetFile(string path)
        {
            return GetFileAsync(path).Result;
        }

        public IDirectory GetDirectory(string path)
        {
            return GetDirectoryAsync(path).Result;
        }

        public IEnumerable<IFile> GetFiles(string path = "")
        {
            return GetFilesAsync(path).Result;
        }

        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            return GetDirectoriesAsync(path).Result;
        }

        public bool FileExists(string path)
        {
            return FileExistsAsync(path).Result;
        }

        public async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                await GetFileAsync(path, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        public bool DirectoryExists(string path)
        {
            return DirectoryExistsAsync(path).Result;
        }

        public async Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                await GetDirectoryAsync(path, cancellationToken);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }

            return true;
        }

        public void CreateDirectory(string path)
        {
            CreateDirectoryAsync(path).Wait();
        }

        public void DeleteDirectory(string path)
        {
            DeleteDirectoryAsync(path).Wait();
        }

        public void DeleteFile(string path)
        {
            DeleteFileAsync(path).Wait();
        }

        public byte[] ReadFile(string path)
        {
            return ReadFileAsync(path).Result;
        }

        public string ReadTextFile(string path)
        {
            return ReadTextFileAsync(path).Result;
        }

        public void WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            WriteFileAsync(path, contents, overwrite).Wait();
        }

        public void WriteFile(string path, string contents, bool overwrite = false)
        {
            WriteFileAsync(path, contents, overwrite).Wait();
        }

        public async Task WriteFileAsync(string path, string contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            await WriteFileAsync(path, Encoding.UTF8.GetBytes(contents), overwrite, cancellationToken);
        }

        public void AppendFile(string path, byte[] contents)
        {
            AppendFileAsync(path, contents).Wait();
        }

        public void AppendFile(string path, string contents)
        {
            AppendFileAsync(path, contents).Wait();
        }

        public async Task AppendFileAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            await AppendFileAsync(path, Encoding.UTF8.GetBytes(contents), cancellationToken);
        }

        public abstract void Dispose();
        public abstract void Connect();
        public abstract Task<IFile> GetFileAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task<IDirectory> GetDirectoryAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<IFile>> GetFilesAsync(string path = "", CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string path = "", CancellationToken cancellationToken = default);
        public abstract Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default);
        public abstract Task WriteFileAsync(string path, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default);
        public abstract Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default);

        protected string GetPath(string virtualPath)
        {
            return PathUtilities.GetPath(virtualPath, RootPath);
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