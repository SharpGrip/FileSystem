using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem
{
    public class FileSystem : IFileSystem, IDisposable
    {
        public IList<IAdapter> Adapters { get; set; } = new List<IAdapter>();

        public FileSystem()
        {
        }

        public FileSystem(IList<IAdapter> adapters)
        {
            Adapters = adapters;
        }

        public void Dispose()
        {
            foreach (var adapter in Adapters)
            {
                adapter.Dispose();
            }
        }

        public IFile GetFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetFile(path);
        }

        public IDirectory GetDirectory(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetDirectory(path);
        }

        public IEnumerable<IFile> GetFiles(string path = "")
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetFiles(path);
        }

        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetDirectories(path);
        }

        public bool FileExists(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.FileExists(path);
        }

        public bool DirectoryExists(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.DirectoryExists(path);
        }

        public Stream CreateFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.CreateFile(path);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            return GetAdapter(prefix).CreateDirectory(path);
        }

        public void DeleteFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            GetAdapter(prefix).DeleteFile(path);
        }

        public void DeleteDirectory(string path, bool recursive = false)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            GetAdapter(prefix).DeleteDirectory(path, recursive);
        }

        public async Task<byte[]> ReadFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            return await GetAdapter(prefix).ReadFile(path);
        }

        public async Task<string> ReadTextFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            return await GetAdapter(prefix).ReadTextFile(path);
        }

        public async Task CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            var sourcePrefix = GetPrefix(sourcePath);
            sourcePath = GetPath(sourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);

            var destinationPrefix = GetPrefix(destinationPath);
            destinationPath = GetPath(destinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);

            await destinationAdapter.WriteFile(destinationPath, await sourceAdapter.ReadFile(sourcePath), overwrite);
        }

        public async Task MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            var sourcePrefix = GetPrefix(sourcePath);
            sourcePath = GetPath(sourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);

            var destinationPrefix = GetPrefix(destinationPath);
            destinationPath = GetPath(destinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);

            await destinationAdapter.WriteFile(destinationPath, await sourceAdapter.ReadFile(sourcePath), overwrite);
            await sourceAdapter.DeleteFile(sourcePath);
        }

        public void WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            GetAdapter(prefix).WriteFile(path, contents, overwrite);
        }

        public void WriteFile(string path, string contents, bool overwrite = false)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            GetAdapter(prefix).WriteFile(path, contents, overwrite);
        }

        public void AppendFile(string path, byte[] contents)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            GetAdapter(prefix).AppendFile(path, contents);
        }

        public void AppendFile(string path, string contents)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);

            GetAdapter(prefix).AppendFile(path, contents);
        }

        public IAdapter GetAdapter(string prefix)
        {
            if (Adapters == null)
            {
                throw new AdaptersNullException("No adapters registered with the file system.");
            }

            if (Adapters.Count == 0)
            {
                throw new AdaptersEmptyException("The provided list of adapters cannot be empty.");
            }

            if (Adapters.All(adapter => adapter.Prefix != prefix))
            {
                var adapters = string.Join(", ", Adapters.Select(adapter => adapter.Prefix).ToArray());

                throw new AdapterNotFoundException($"No adapter found with prefix '{prefix}'. Registered adapters are: {adapters}.");
            }

            return Adapters.First(adapter => adapter.Prefix == prefix);
        }

        private static string GetPrefix(string path)
        {
            return ResolvePrefixAndPath(path)[0];
        }

        private static string GetPath(string path)
        {
            return ResolvePrefixAndPath(path)[1];
        }

        private static string[] ResolvePrefixAndPath(string path)
        {
            if (!path.Contains("://"))
            {
                throw new PrefixNotFoundException($"No prefix found in path '{path}'.");
            }

            return path.Split(new[] {"://"}, StringSplitOptions.None);
        }
    }
}