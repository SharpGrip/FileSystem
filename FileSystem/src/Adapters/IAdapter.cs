using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Configuration;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters
{
    public interface IAdapter : IDisposable
    {
        public string Prefix { get; }
        public string RootPath { get; }
        public string Name { get; }
        public IAdapterConfiguration AdapterConfiguration { get; }
        public void Connect();
        public void ClearCache();
        public Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default);
        public Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default);
    }
}