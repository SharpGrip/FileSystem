using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

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
            path = PrependRootPath(path);

            try
            {
                var file = new FileInfo(path);

                if (!file.Exists)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return new FileModel(file);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public IDirectory GetDirectory(string path)
        {
            path = PrependRootPath(path);

            try
            {
                var directory = new DirectoryInfo(path);

                if (!directory.Exists)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return new DirectoryModel(directory);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public IEnumerable<IFile> GetFiles(string path = "")
        {
            path = PrependRootPath(path);
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            return directory.GetFiles().Select(item => GetFile(item.FullName)).ToList();
        }

        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            path = PrependRootPath(path);
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
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

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(PrependRootPath(path));
        }

        public void DeleteFile(string path)
        {
            File.Delete(PrependRootPath(path));
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(PrependRootPath(path), true);
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
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await using var fileStream = new FileStream(path, FileMode.Create);

            await fileStream.WriteAsync(contents);
        }

        public async Task WriteFile(string path, string contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(path, Prefix);
            }

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