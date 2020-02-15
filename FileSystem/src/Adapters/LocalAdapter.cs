using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters
{
    public class LocalAdapter : Adapter, IAdapter
    {
        public LocalAdapter(string prefix, string rootPath) : base(prefix, rootPath)
        {
        }

        public void Dispose()
        {
        }

        public void Connect()
        {
        }

        public IFile GetFile(string path)
        {
            var file = new FileInfo(PrependRootPath(path));

            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }

            return new FileModel(file);
        }

        public IDirectory GetDirectory(string path)
        {
            var directory = new DirectoryInfo(PrependRootPath(path));

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            return new DirectoryModel(directory);
        }

        public IEnumerable<IFile> GetFiles(string path = "")
        {
            var directory = new DirectoryInfo(PrependRootPath(path));

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            return directory.GetFiles().Select(item => GetFile(item.FullName)).ToList();
        }

        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            var directory = new DirectoryInfo(PrependRootPath(path));

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            return directory.GetDirectories().Select(item => GetDirectory(item.FullName)).ToList();
        }

        public bool FileExists(string path)
        {
            return File.Exists(PrependRootPath(path));
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(PrependRootPath(path));
        }

        public Stream CreateFile(string path)
        {
            return File.Create(PrependRootPath(path));
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(PrependRootPath(path));
        }

        public Task DeleteFile(string path)
        {
            return Task.Factory.StartNew(() => File.Delete(PrependRootPath(path)));
        }

        public Task DeleteDirectory(string path, bool recursive = false)
        {
            return Task.Factory.StartNew(() => Directory.Delete(PrependRootPath(path), recursive));
        }

        public async Task<byte[]> ReadFile(string path)
        {
            await using var fileStream = new FileStream(PrependRootPath(path), FileMode.Open);
            var fileContents = new byte[fileStream.Length];

            await fileStream.ReadAsync(fileContents, 0, (int) fileStream.Length);

            return fileContents;
        }

        public async Task<string> ReadTextFile(string path)
        {
            using var streamReader = new StreamReader(PrependRootPath(path));

            return await streamReader.ReadToEndAsync();
        }

        public async Task WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            await using var fileStream = new FileStream(PrependRootPath(path), overwrite ? FileMode.Create : FileMode.CreateNew);

            await fileStream.WriteAsync(contents);
        }

        public async Task WriteFile(string path, string contents, bool overwrite = false)
        {
            await using var fileStream = new FileStream(PrependRootPath(path), overwrite ? FileMode.Create : FileMode.CreateNew);
            await using var streamWriter = new StreamWriter(fileStream);

            await streamWriter.WriteAsync(contents);
        }

        public async Task AppendFile(string path, byte[] contents)
        {
            await using var fileStream = new FileStream(path, FileMode.Append);

            await fileStream.WriteAsync(contents);
        }

        public async Task AppendFile(string path, string contents)
        {
            await using var fileStream = new FileStream(PrependRootPath(path), FileMode.Append);
            await using var streamWriter = new StreamWriter(fileStream);

            await streamWriter.WriteAsync(contents);
        }
    }
}