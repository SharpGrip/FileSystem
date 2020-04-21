using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Models;
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

        protected string PrependRootPath(string path)
        {
            return Path.Combine(RootPath, path);
        }

        public bool FileExists(string path)
        {
            try
            {
                GetFile(path);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        public bool DirectoryExists(string path)
        {
            try
            {
                GetDirectory(path);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }

            return true;
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

        public async Task WriteFileAsync(string path, string contents, bool overwrite = false)
        {
            await WriteFileAsync(path, Encoding.UTF8.GetBytes(contents), overwrite);
        }

        public void AppendFile(string path, byte[] contents)
        {
            AppendFileAsync(path, contents).Wait();
        }

        public void AppendFile(string path, string contents)
        {
            AppendFileAsync(path, contents).Wait();
        }

        public async Task AppendFileAsync(string path, string contents)
        {
            await AppendFileAsync(path, Encoding.UTF8.GetBytes(contents));
        }

        public abstract void Dispose();
        public abstract void Connect();
        public abstract IFile GetFile(string path);
        public abstract IDirectory GetDirectory(string path);
        public abstract IEnumerable<IFile> GetFiles(string path = "");
        public abstract IEnumerable<IDirectory> GetDirectories(string path = "");
        public abstract void CreateDirectory(string path);
        public abstract void DeleteDirectory(string path);
        public abstract void DeleteFile(string path);
        public abstract Task<byte[]> ReadFileAsync(string path);
        public abstract Task<string> ReadTextFileAsync(string path);
        public abstract Task WriteFileAsync(string path, byte[] contents, bool overwrite = false);
        public abstract Task AppendFileAsync(string path, byte[] contents);
    }
}