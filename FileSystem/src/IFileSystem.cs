using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpGrip.FileSystem
{
    public interface IFileSystem
    {
        public FileInfo GetFile(string path);

        public DirectoryInfo GetDirectory(string path);

        public IEnumerable<FileInfo> GetFiles(string path = "");

        public IEnumerable<DirectoryInfo> GetDirectories(string path = "");

        public bool FileExists(string path);

        public bool DirectoryExists(string path);

        public FileStream CreateFile(string path);

        public DirectoryInfo CreateDirectory(string path);

        public void DeleteFile(string path);

        public void DeleteDirectory(string path, bool recursive = false);

        public Task<byte[]> ReadFile(string path);

        public Task<string> ReadTextFile(string path);

        public Task CopyFile(string sourcePath, string destinationPath, bool overwrite = false);

        public Task MoveFile(string sourcePath, string destinationPath, bool overwrite = false);

        public void WriteFile(string path, byte[] contents, bool overwrite = false);

        public void WriteFile(string path, string contents, bool overwrite = false);

        public void AppendFile(string path, byte[] contents);

        public void AppendFile(string path, string contents);
    }
}