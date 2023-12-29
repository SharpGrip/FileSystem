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
        public IFile GetFile(string virtualPath);
        public Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public IDirectory GetDirectory(string virtualPath);
        public Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public IEnumerable<IFile> GetFiles(string virtualPath = "");
        public Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public IEnumerable<IDirectory> GetDirectories(string virtualPath = "");
        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public bool FileExists(string virtualPath);
        public Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default);
        public bool DirectoryExists(string virtualPath);
        public Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default);
        public void CreateDirectory(string virtualPath);
        public Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public void DeleteFile(string virtualPath);
        public Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public void DeleteDirectory(string virtualPath);
        public Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public byte[] ReadFile(string virtualPath);
        public Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task<byte[]> ReadFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public string ReadTextFile(string virtualPath);
        public Task<string> ReadTextFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default);
        public void WriteFile(string virtualPath, byte[] contents, bool overwrite = false);
        public Task WriteFileAsync(string virtualPath, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default);
        public void WriteFile(string virtualPath, string contents, bool overwrite = false);
        public Task WriteFileAsync(string virtualPath, string contents, bool overwrite = false, CancellationToken cancellationToken = default);
        public Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default);
        public void AppendFile(string virtualPath, byte[] contents);
        public Task AppendFileAsync(string virtualPath, byte[] contents, CancellationToken cancellationToken = default);
        public void AppendFile(string virtualPath, string contents);
        public Task AppendFileAsync(string virtualPath, string contents, CancellationToken cancellationToken = default);
    }
}