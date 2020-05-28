using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// Returns an adapter by prefix.
        /// </summary>
        /// <param name="prefix">The adapter's prefix.</param>
        /// <returns>The adapter.</returns>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public IAdapter GetAdapter(string prefix)
        {
            if (Adapters.Count == 0)
            {
                throw new NoAdaptersRegisteredException();
            }

            var duplicateAdapters = Adapters.GroupBy(adapter => adapter.Prefix).Where(grouping => grouping.Count() > 1).ToList();

            if (duplicateAdapters.Any())
            {
                throw new DuplicateAdapterPrefixException(duplicateAdapters, Adapters);
            }

            if (Adapters.All(adapter => adapter.Prefix != prefix))
            {
                throw new AdapterNotFoundException(prefix, Adapters);
            }

            return Adapters.First(adapter => adapter.Prefix == prefix);
        }

        /// <summary>
        /// Return a file.
        /// </summary>
        /// <param name="path">The path (including prefix) to the file.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public IFile GetFile(string path)
        {
            return GetFileAsync(path).Result;
        }

        /// <summary>
        /// Return a file.
        /// </summary>
        /// <param name="path">The path (including prefix) to the file.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task<IFile> GetFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetFileAsync(path, cancellationToken);
        }

        /// <summary>
        /// Returns a directory.
        /// </summary>
        /// <param name="path">The path (including prefix) to the directory.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IDirectory GetDirectory(string path)
        {
            return GetDirectoryAsync(path).Result;
        }

        /// <summary>
        /// Returns a directory.
        /// </summary>
        /// <param name="path">The path (including prefix) to the directory.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IDirectory> GetDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetDirectoryAsync(path, cancellationToken);
        }

        /// <summary>
        /// Returns files present at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to list the files at.</param>
        /// <returns>The files present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IEnumerable<IFile> GetFiles(string path = "")
        {
            return GetFilesAsync(path).Result;
        }

        /// <summary>
        /// Returns files present at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to list the files at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The files present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IEnumerable<IFile>> GetFilesAsync(string path = "", CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetFilesAsync(path, cancellationToken);
        }

        /// <summary>
        /// Returns directories present at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to list the directories at.</param>
        /// <returns>The directories present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            return GetDirectoriesAsync(path).Result;
        }

        /// <summary>
        /// Returns directories present at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to list the directories at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The directories present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string path = "", CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return adapter.GetDirectoriesAsync(path, cancellationToken);
        }

        /// <summary>
        /// Checks if a file exists at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to check.</param>
        /// <returns>True if the file exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public bool FileExists(string path)
        {
            return FileExistsAsync(path).Result;
        }

        /// <summary>
        /// Checks if a file exists at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to check.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>True if the file exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return await adapter.FileExistsAsync(path, cancellationToken);
        }

        /// <summary>
        /// Checks if a directory exists at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to check.</param>
        /// <returns>True if the directory exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public bool DirectoryExists(string path)
        {
            return DirectoryExistsAsync(path).Result;
        }

        /// <summary>
        /// Checks if a directory exists at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) to check.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>True if the directory exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public async Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return await adapter.DirectoryExistsAsync(path, cancellationToken);
        }

        /// <summary>
        /// Creates a directory at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to create the directory at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryExistsException">Thrown if the directory exists at the given path.</exception>
        public void CreateDirectory(string path)
        {
            CreateDirectoryAsync(path).Wait();
        }

        /// <summary>
        /// Creates a directory at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to create the directory at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryExistsException">Thrown if the directory exists at the given path.</exception>
        public async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.CreateDirectoryAsync(path, cancellationToken);
        }

        /// <summary>
        /// Deletes a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to delete the file at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void DeleteFile(string path)
        {
            DeleteFileAsync(path).Wait();
        }

        /// <summary>
        /// Deletes a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to delete the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.DeleteFileAsync(path, cancellationToken);
        }

        /// <summary>
        /// Deletes a directory at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to delete the directory at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public void DeleteDirectory(string path)
        {
            DeleteDirectoryAsync(path).Wait();
        }

        /// <summary>
        /// Deletes a directory at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to delete the directory at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public async Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.DeleteDirectoryAsync(path, cancellationToken);
        }

        /// <summary>
        /// Reads a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to read the file at.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public byte[] ReadFile(string path)
        {
            return ReadFileAsync(path).Result;
        }

        /// <summary>
        /// Reads a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to read the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return await adapter.ReadFileAsync(path, cancellationToken);
        }

        /// <summary>
        /// Reads a text file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to read the file at.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public string ReadTextFile(string path)
        {
            return ReadTextFileAsync(path).Result;
        }

        /// <summary>
        /// Reads a text file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to read the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            return await adapter.ReadTextFileAsync(path, cancellationToken);
        }

        /// <summary>
        /// Copies a file from a source path to a destination path.
        /// </summary>
        /// <param name="sourcePath">The path (including prefix) where to copy the file from.</param>
        /// <param name="destinationPath">The path (including prefix) where to copy the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            CopyFileAsync(sourcePath, destinationPath, overwrite).Wait();
        }

        /// <summary>
        /// Copies a file from a source path to a destination path.
        /// </summary>
        /// <param name="sourcePath">The path (including prefix) where to copy the file from.</param>
        /// <param name="destinationPath">The path (including prefix) where to copy the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task CopyFileAsync(
            string sourcePath,
            string destinationPath,
            bool overwrite = false,
            CancellationToken cancellationToken = default
        )
        {
            var sourcePrefix = GetPrefix(sourcePath);
            sourcePath = GetPath(sourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);
            sourceAdapter.Connect();

            var destinationPrefix = GetPrefix(destinationPath);
            destinationPath = GetPath(destinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);
            destinationAdapter.Connect();

            var contents = await sourceAdapter.ReadFileAsync(sourcePath, cancellationToken);
            await destinationAdapter.WriteFileAsync(destinationPath, contents, overwrite, cancellationToken);
        }

        /// <summary>
        /// Moves a file from a source path to a destination path.
        /// </summary>
        /// <param name="sourcePath">The path (including prefix) where to move the file from.</param>
        /// <param name="destinationPath">The path (including prefix) where to move the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            MoveFileAsync(sourcePath, destinationPath, overwrite).Wait();
        }

        /// <summary>
        /// Moves a file from a source path to a destination path.
        /// </summary>
        /// <param name="sourcePath">The path (including prefix) where to move the file from.</param>
        /// <param name="destinationPath">The path (including prefix) where to move the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task MoveFileAsync(
            string sourcePath,
            string destinationPath,
            bool overwrite = false,
            CancellationToken cancellationToken = default
        )
        {
            var sourcePrefix = GetPrefix(sourcePath);
            sourcePath = GetPath(sourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);
            sourceAdapter.Connect();

            var destinationPrefix = GetPrefix(destinationPath);
            destinationPath = GetPath(destinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);
            destinationAdapter.Connect();

            var contents = await sourceAdapter.ReadFileAsync(sourcePath, cancellationToken);
            await destinationAdapter.WriteFileAsync(destinationPath, contents, overwrite, cancellationToken);
            await sourceAdapter.DeleteFileAsync(sourcePath, cancellationToken);
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            WriteFileAsync(path, contents, overwrite).Wait();
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task WriteFileAsync(
            string path,
            byte[] contents,
            bool overwrite = false,
            CancellationToken cancellationToken = default
        )
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.WriteFileAsync(path, contents, overwrite, cancellationToken);
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void WriteFile(string path, string contents, bool overwrite = false)
        {
            WriteFileAsync(path, contents, overwrite).Wait();
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task WriteFileAsync(
            string path,
            string contents,
            bool overwrite = false,
            CancellationToken cancellationToken = default
        )
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.WriteFileAsync(path, contents, overwrite, cancellationToken);
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void AppendFile(string path, byte[] contents)
        {
            AppendFileAsync(path, contents).Wait();
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.AppendFileAsync(path, contents, cancellationToken);
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void AppendFile(string path, string contents)
        {
            AppendFileAsync(path, contents).Wait();
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="path">The path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task AppendFileAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            var prefix = GetPrefix(path);
            path = GetPath(path);
            var adapter = GetAdapter(prefix);
            adapter.Connect();

            await adapter.AppendFileAsync(path, contents, cancellationToken);
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