using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters
{
    public interface IAdapter : IDisposable
    {
        string Prefix { get; }
        string RootPath { get; }
        public void Connect();
        IFile GetFile(string path);
        IDirectory GetDirectory(string path);
        IEnumerable<IFile> GetFiles(string path = "");
        IEnumerable<IDirectory> GetDirectories(string path = "");
        bool FileExists(string path);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        void DeleteFile(string path);
        void DeleteDirectory(string path);
        byte[] ReadFile(string path);
        Task<byte[]> ReadFileAsync(string path);
        string ReadTextFile(string path);
        Task<string> ReadTextFileAsync(string path);
        void WriteFile(string path, byte[] contents, bool overwrite = false);
        Task WriteFileAsync(string path, byte[] contents, bool overwrite = false);
        void WriteFile(string path, string contents, bool overwrite = false);
        Task WriteFileAsync(string path, string contents, bool overwrite = false);
        void AppendFile(string path, byte[] contents);
        Task AppendFileAsync(string path, byte[] contents);
        void AppendFile(string path, string contents);
        Task AppendFileAsync(string path, string contents);
    }
}