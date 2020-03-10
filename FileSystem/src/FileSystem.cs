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
        /// <summary>
        /// The adapters.
        /// </summary>
        public IList<IAdapter> Adapters { get; set; } = new List<IAdapter>();

        /// <summary>
        /// FileSystem constructor.
        /// </summary>
        public FileSystem()
        {
        }

        /// <summary>
        /// FileSystem constructor.
        /// </summary>
        /// <param name="adapters">The adapters.</param>
        public FileSystem(IList<IAdapter> adapters)
        {
            Adapters = adapters;
        }

        /// <summary>
        /// Disposes the attached adapters.
        /// </summary>
        public void Dispose()
        {
            foreach (var adapter in Adapters)
            {
                adapter.Dispose();
            }
        }

        /// <summary>
        /// Return a file.
        /// </summary>
        /// <param name="path">The path (including prefix) to the file.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public IFile GetFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetFile(path);
        }

        /// <summary>
        /// Returns a directory.
        /// </summary>
        /// <param name="path">The path (including prefix) to the directory.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public IDirectory GetDirectory(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetDirectory(path);
        }

        /// <summary>
        /// Returns files present at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to list the files at.</param>
        /// <returns>The files present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public IEnumerable<IFile> GetFiles(string path = "")
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetFiles(path);
        }

        /// <summary>
        /// Returns directories present at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to list the directories at.</param>
        /// <returns>The directories present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetDirectories(path);
        }

        /// <summary>
        /// Checks if a file exists at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to check.</param>
        /// <returns>True if the file exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public bool FileExists(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.FileExists(path);
        }

        /// <summary>
        /// Checks if a directory exists at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to check.</param>
        /// <returns>True if the directory exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public bool DirectoryExists(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.DirectoryExists(path);
        }

        /// <summary>
        /// Creates a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to create the file at.</param>
        /// <returns>The created file.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public Stream CreateFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.CreateFile(path);
        }

        /// <summary>
        /// Creates a directory at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to create the directory at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public void CreateDirectory(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            adapter.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to delete the file at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public void DeleteFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            adapter.DeleteFile(path);
        }

        /// <summary>
        /// Deletes a directory at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to delete the directory at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public void DeleteDirectory(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            adapter.DeleteDirectory(path);
        }

        /// <summary>
        /// Reads a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to read the file at.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public async Task<byte[]> ReadFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return await adapter.ReadFile(path);
        }

        /// <summary>
        /// Reads a text file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to read the file at.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public async Task<string> ReadTextFile(string path)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return await adapter.ReadTextFile(path);
        }

        /// <summary>
        /// Copies a file from a source path to a destination path.
        /// </summary>
        /// <param name="sourcePath">The path (including prefix) where to copy the file from.</param>
        /// <param name="destinationPath">The path (including prefix) where to copy the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            var sourcePrefix = GetPrefix(sourcePath);
            sourcePath = GetPath(sourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);
            sourceAdapter.Connect();

            var destinationPrefix = GetPrefix(destinationPath);
            destinationPath = GetPath(destinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);
            destinationAdapter.Connect();

            await destinationAdapter.WriteFile(destinationPath, await sourceAdapter.ReadFile(sourcePath), overwrite);
        }

        /// <summary>
        /// Moves a file from a source path to a destination path.
        /// </summary>
        /// <param name="sourcePath">The path (including prefix) where to move the file from.</param>
        /// <param name="destinationPath">The path (including prefix) where to move the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            var sourcePrefix = GetPrefix(sourcePath);
            sourcePath = GetPath(sourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);
            sourceAdapter.Connect();

            var destinationPrefix = GetPrefix(destinationPath);
            destinationPath = GetPath(destinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);
            destinationAdapter.Connect();

            await destinationAdapter.WriteFile(destinationPath, await sourceAdapter.ReadFile(sourcePath), overwrite);
            sourceAdapter.DeleteFile(sourcePath);
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.WriteFile(path, contents, overwrite);
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task WriteFile(string path, string contents, bool overwrite = false)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.WriteFile(path, contents, overwrite);
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public async Task AppendFile(string path, byte[] contents)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.AppendFile(path, contents);
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        public async Task AppendFile(string path, string contents)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.AppendFile(path, contents);
        }

        /// <summary>
        /// Returns an adapter by prefix.
        /// </summary>
        /// <param name="prefix">The adapter's prefix.</param>
        /// <returns>The adapter.</returns>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        public IAdapter GetAdapter(string prefix)
        {
            if (Adapters.Count == 0)
            {
                throw new NoAdaptersRegisteredException();
            }

            if (Adapters.All(adapter => adapter.Prefix != prefix))
            {
                var adapters = string.Join(", ", Adapters.Select(adapter => adapter.Prefix).ToArray());

                throw new AdapterNotFoundException(prefix, adapters);
            }

            return Adapters.First(adapter => adapter.Prefix == prefix);
        }

        /// <summary>
        /// Returns the prefix from a prefixed path.
        /// </summary>
        /// <param name="path">The prefixed path.</param>
        /// <returns>The prefix.</returns>
        private static string GetPrefix(string path)
        {
            return ResolvePrefixAndPath(path)[0];
        }

        /// <summary>
        /// Returns the path from a prefixed path.
        /// </summary>
        /// <param name="path">The prefixed path.</param>
        /// <returns>The path.</returns>
        private static string GetPath(string path)
        {
            return ResolvePrefixAndPath(path)[1];
        }

        /// <summary>
        /// Resolves the prefix and path from a prefixed path.
        /// </summary>
        /// <param name="path">The prefixed path.</param>
        /// <returns>The prefix and path.</returns>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        private static string[] ResolvePrefixAndPath(string path)
        {
            if (!path.Contains("://"))
            {
                throw new PrefixNotFoundInPathException(path);
            }

            return path.Split(new[] {"://"}, StringSplitOptions.None);
        }
    }
}