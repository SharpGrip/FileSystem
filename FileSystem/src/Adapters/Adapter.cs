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

        public string ReadTextFile(string virtualPath)
        {
            return ReadTextFileAsync(virtualPath).Result;
        }

        public void WriteFile(string virtualPath, byte[] contents, bool overwrite = false)
        {
            WriteFileAsync(virtualPath, contents, overwrite).Wait();
        }

        public void WriteFile(string virtualPath, string contents, bool overwrite = false)
        {
            WriteFileAsync(virtualPath, contents, overwrite).Wait();
        }

        public async Task WriteFileAsync(string virtualPath, string contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            await WriteFileAsync(virtualPath, Encoding.UTF8.GetBytes(contents), overwrite, cancellationToken);
        }

        public void AppendFile(string virtualPath, byte[] contents)
        {
            AppendFileAsync(virtualPath, contents).Wait();
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
        public abstract Task<byte[]> ReadFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<string> ReadTextFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task WriteFileAsync(string virtualPath, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default);
        public abstract Task AppendFileAsync(string virtualPath, byte[] contents, CancellationToken cancellationToken = default);

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