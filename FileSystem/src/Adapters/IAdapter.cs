using System;
using System.Collections.Generic;
using System.IO;
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
        Stream CreateFile(string path);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
        Task<byte[]> ReadFile(string path);
        Task<string> ReadTextFile(string path);
        Task WriteFile(string path, byte[] contents, bool overwrite = false);
        Task WriteFile(string path, string contents, bool overwrite = false);
        Task AppendFile(string sourcePath, byte[] contents);
        Task AppendFile(string sourcePath, string contents);
    }
}