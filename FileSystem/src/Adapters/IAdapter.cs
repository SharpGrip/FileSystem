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
        string Prefix { get; }
        string RootPath { get; }
        public IAdapterConfiguration AdapterConfiguration { get; }
        public void Connect();
        public void ClearCache();
        IFile GetFile(string virtualPath);
        Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        IDirectory GetDirectory(string virtualPath);
        Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        IEnumerable<IFile> GetFiles(string virtualPath = "");
        Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        IEnumerable<IDirectory> GetDirectories(string virtualPath = "");
        Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        bool FileExists(string virtualPath);
        Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default);
        bool DirectoryExists(string virtualPath);
        Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default);
        void CreateDirectory(string virtualPath);
        Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        void DeleteFile(string virtualPath);
        Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        void DeleteDirectory(string virtualPath);
        Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        byte[] ReadFile(string virtualPath);
        Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default);
        Task<byte[]> ReadFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        string ReadTextFile(string virtualPath);
        Task<string> ReadTextFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default);
        void WriteFile(string virtualPath, byte[] contents, bool overwrite = false);
        Task WriteFileAsync(string virtualPath, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default);
        void WriteFile(string virtualPath, string contents, bool overwrite = false);
        Task WriteFileAsync(string virtualPath, string contents, bool overwrite = false, CancellationToken cancellationToken = default);
        Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default);
        void AppendFile(string virtualPath, byte[] contents);
        Task AppendFileAsync(string virtualPath, byte[] contents, CancellationToken cancellationToken = default);
        void AppendFile(string virtualPath, string contents);
        Task AppendFileAsync(string virtualPath, string contents, CancellationToken cancellationToken = default);
    }
}