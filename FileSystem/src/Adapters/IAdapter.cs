using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpGrip.FileSystem.Adapters
{
    public interface IAdapter
    {
        string Prefix { get; }
        FileInfo GetFile(string path);
        DirectoryInfo GetDirectory(string path);
        IEnumerable<FileInfo> GetFiles(string path = "");
        IEnumerable<DirectoryInfo> GetDirectories(string path = "");
        bool FileExists(string path);
        bool DirectoryExists(string path);
        FileStream CreateFile(string path);
        DirectoryInfo CreateDirectory(string path);
        Task DeleteDirectory(string path, bool recursive);
        Task DeleteFile(string path);
        Task<byte[]> ReadFile(string path);
        Task<string> ReadTextFile(string path);
        Task WriteFile(string path, byte[] contents, bool overwrite = false);
        Task WriteFile(string path, string contents, bool overwrite = false);
        Task AppendFile(string sourcePath, byte[] contents);
        Task AppendFile(string sourcePath, string contents);
    }
}