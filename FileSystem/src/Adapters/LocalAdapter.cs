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
    public class LocalAdapter : Adapter
    {
        public LocalAdapter(string prefix, string rootPath) : base(prefix, rootPath)
        {
        }

        public override void Dispose()
        {
        }

        public override void Connect()
        {
        }

        public override IFile GetFile(string path)
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

        public override IDirectory GetDirectory(string path)
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

        public override IEnumerable<IFile> GetFiles(string path = "")
        {
            path = PrependRootPath(path);
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            return directory.GetFiles().Select(item => GetFile(item.FullName)).ToList();
        }

        public override IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            path = PrependRootPath(path);
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            return directory.GetDirectories().Select(item => GetDirectory(item.FullName)).ToList();
        }

        public override void CreateDirectory(string path)
        {
            Directory.CreateDirectory(PrependRootPath(path));
        }

        public override void DeleteFile(string path)
        {
            File.Delete(PrependRootPath(path));
        }

        public override void DeleteDirectory(string path)
        {
            Directory.Delete(PrependRootPath(path), true);
        }

        public override async Task<byte[]> ReadFileAsync(string path)
        {
            await using var fileStream = new FileStream(PrependRootPath(path), FileMode.Open);
            var fileContents = new byte[fileStream.Length];

            await fileStream.ReadAsync(fileContents, 0, (int) fileStream.Length);

            return fileContents;
        }

        public override async Task<string> ReadTextFileAsync(string path)
        {
            using var streamReader = new StreamReader(PrependRootPath(path));

            return await streamReader.ReadToEndAsync();
        }

        public override async Task WriteFileAsync(string path, byte[] contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await using var fileStream = new FileStream(path, FileMode.Create);

            await fileStream.WriteAsync(contents);
        }

        public override async Task AppendFileAsync(string path, byte[] contents)
        {
            if (!FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await using var fileStream = new FileStream(path, FileMode.Append);

            await fileStream.WriteAsync(contents);
        }
    }
}