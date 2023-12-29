using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Configuration;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;

namespace SharpGrip.FileSystem
{
    public class FileSystem : IFileSystem, IDisposable
    {
        /// <summary>
        /// The adapters.
        /// </summary>
        public IList<IAdapter> Adapters { get; set; } = new List<IAdapter>();

        /// <summary>
        /// Configuration.
        /// </summary>
        public FileSystemConfiguration Configuration { get; } = new FileSystemConfiguration();

        /// <summary>
        /// The file system logger.
        /// </summary>
        private ILogger Logger { get; set; } = NullLogger<FileSystem>.Instance;

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
        /// <param name="configuration">The configuration delegate used to configure the file system.</param>
        public FileSystem(IList<IAdapter> adapters, Action<FileSystemConfiguration>? configuration = null)
        {
            Adapters = adapters;

            var fileSystemConfiguration = new FileSystemConfiguration();
            configuration?.Invoke(fileSystemConfiguration);

            Configuration = fileSystemConfiguration;

            if (Configuration.EnableLogging)
            {
                Logger = Configuration.Logger ?? LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Trace);
                }).CreateLogger(GetType().FullName ?? GetType().Name);
            }
        }

        /// <summary>
        /// Disposes the attached adapters.
        /// </summary>
        public void Dispose()
        {
            Logger.LogStartDisposingAdapters(Adapters.Count);

            foreach (var adapter in Adapters)
            {
                Logger.LogStartDisposingAdapter(adapter);

                adapter.Dispose();

                Logger.LogFinishedDisposingAdapter(adapter);
            }

            Logger.LogFinishedDisposingAdapters(Adapters.Count);
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
            Logger.LogStartRetrievingAdapter(prefix);

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

            var adapter = Adapters.First(adapter => adapter.Prefix == prefix);

            Logger.LogFinishedRetrievingAdapter(adapter);

            return adapter;
        }

        /// <summary>
        /// Return a file.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to the file.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public IFile GetFile(string virtualPath)
        {
            return GetFileAsync(virtualPath).Result;
        }

        /// <summary>
        /// Return a file.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to the file.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            Logger.LogStartConnectingAdapter(adapter);
            adapter.Connect();
            Logger.LogFinishedConnectingAdapter(adapter);

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return adapter.GetFileAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Returns a directory.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to the directory.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IDirectory GetDirectory(string virtualPath)
        {
            return GetDirectoryAsync(virtualPath).Result;
        }

        /// <summary>
        /// Returns a directory.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to the directory.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return adapter.GetDirectoryAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Returns files present at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to list the files at.</param>
        /// <returns>The files present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IEnumerable<IFile> GetFiles(string virtualPath = "")
        {
            return GetFilesAsync(virtualPath).Result;
        }

        /// <summary>
        /// Returns files present at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to list the files at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The files present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return adapter.GetFilesAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Returns directories present at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to list the directories at.</param>
        /// <returns>The directories present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IEnumerable<IDirectory> GetDirectories(string virtualPath = "")
        {
            return GetDirectoriesAsync(virtualPath).Result;
        }

        /// <summary>
        /// Returns directories present at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to list the directories at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The directories present at the provided path.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return adapter.GetDirectoriesAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Checks if a file exists at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to check.</param>
        /// <returns>True if the file exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public bool FileExists(string virtualPath)
        {
            return FileExistsAsync(virtualPath).Result;
        }

        /// <summary>
        /// Checks if a file exists at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to check.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>True if the file exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public async Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return await adapter.FileExistsAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Checks if a directory exists at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to check.</param>
        /// <returns>True if the directory exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public bool DirectoryExists(string virtualPath)
        {
            return DirectoryExistsAsync(virtualPath).Result;
        }

        /// <summary>
        /// Checks if a directory exists at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) to check.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>True if the directory exists at the provided path, False otherwise.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public async Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return await adapter.DirectoryExistsAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Creates a directory at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to create the directory at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryExistsException">Thrown if the directory exists at the given path.</exception>
        public void CreateDirectory(string virtualPath)
        {
            CreateDirectoryAsync(virtualPath).Wait();
        }

        /// <summary>
        /// Creates a directory at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to create the directory at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="DirectoryExistsException">Thrown if the directory exists at the given path.</exception>
        public async Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.CreateDirectoryAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Deletes a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to delete the file at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void DeleteFile(string virtualPath)
        {
            DeleteFileAsync(virtualPath).Wait();
        }

        /// <summary>
        /// Deletes a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to delete the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.DeleteFileAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Deletes a directory at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to delete the directory at.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public void DeleteDirectory(string virtualPath)
        {
            DeleteDirectoryAsync(virtualPath).Wait();
        }

        /// <summary>
        /// Deletes a directory at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to delete the directory at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.DeleteDirectoryAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Reads a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to read the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return await adapter.ReadFileStreamAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Reads a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to read the file at.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public byte[] ReadFile(string virtualPath)
        {
            return ReadFileAsync(virtualPath).Result;
        }

        /// <summary>
        /// Reads a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to read the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task<byte[]> ReadFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return await adapter.ReadFileAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Reads a text file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to read the file at.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public string ReadTextFile(string virtualPath)
        {
            return ReadTextFileAsync(virtualPath).Result;
        }

        /// <summary>
        /// Reads a text file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to read the file at.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task<string> ReadTextFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            return await adapter.ReadTextFileAsync(virtualPath, cancellationToken);
        }

        /// <summary>
        /// Copies a file from a source path to a destination path.
        /// </summary>
        /// <param name="virtualSourcePath">The virtual path (including prefix) where to copy the file from.</param>
        /// <param name="virtualDestinationPath">The virtual path (including prefix) where to copy the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void CopyFile(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false)
        {
            CopyFileAsync(virtualSourcePath, virtualDestinationPath, overwrite).Wait();
        }

        /// <summary>
        /// Copies a file from a source path to a destination path.
        /// </summary>
        /// <param name="virtualSourcePath">The virtual path (including prefix) where to copy the file from.</param>
        /// <param name="virtualDestinationPath">The virtual path (including prefix) where to copy the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task CopyFileAsync(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var sourcePrefix = PathUtilities.GetPrefix(virtualSourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);

            var destinationPrefix = PathUtilities.GetPrefix(virtualDestinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);

            sourceAdapter.Connect();

            if (!sourceAdapter.AdapterConfiguration.EnableCache)
            {
                sourceAdapter.ClearCache();
            }

            destinationAdapter.Connect();

            if (!destinationAdapter.AdapterConfiguration.EnableCache)
            {
                destinationAdapter.ClearCache();
            }

            using var fileStream = await sourceAdapter.ReadFileStreamAsync(virtualSourcePath, cancellationToken);
            await destinationAdapter.WriteFileAsync(virtualDestinationPath, fileStream, overwrite, cancellationToken);
        }

        /// <summary>
        /// Moves a file from a source path to a destination path.
        /// </summary>
        /// <param name="virtualSourcePath">The virtual path (including prefix) where to move the file from.</param>
        /// <param name="virtualDestinationPath">The virtual path (including prefix) where to move the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void MoveFile(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false)
        {
            MoveFileAsync(virtualSourcePath, virtualDestinationPath, overwrite).Wait();
        }

        /// <summary>
        /// Moves a file from a source path to a destination path.
        /// </summary>
        /// <param name="virtualSourcePath">The virtual path (including prefix) where to move the file from.</param>
        /// <param name="virtualDestinationPath">The virtual path (including prefix) where to move the file to.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public async Task MoveFileAsync(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var sourcePrefix = PathUtilities.GetPrefix(virtualSourcePath);
            var sourceAdapter = GetAdapter(sourcePrefix);

            var destinationPrefix = PathUtilities.GetPrefix(virtualDestinationPath);
            var destinationAdapter = GetAdapter(destinationPrefix);

            sourceAdapter.Connect();

            if (!sourceAdapter.AdapterConfiguration.EnableCache)
            {
                sourceAdapter.ClearCache();
            }

            destinationAdapter.Connect();

            if (!destinationAdapter.AdapterConfiguration.EnableCache)
            {
                destinationAdapter.ClearCache();
            }

            using var fileStream = await sourceAdapter.ReadFileStreamAsync(virtualSourcePath, cancellationToken);
            await destinationAdapter.WriteFileAsync(virtualDestinationPath, fileStream, overwrite, cancellationToken);

            fileStream.Dispose();

            await sourceAdapter.DeleteFileAsync(virtualSourcePath, cancellationToken);
        }

        /// <summary>
        /// Writes stream contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the byte array contents to.</param>
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
        public async Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.WriteFileAsync(virtualPath, contents, overwrite, cancellationToken);
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void WriteFile(string virtualPath, byte[] contents, bool overwrite = false)
        {
            WriteFileAsync(virtualPath, contents, overwrite).Wait();
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the byte array contents to.</param>
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
        public async Task WriteFileAsync(string virtualPath, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.WriteFileAsync(virtualPath, contents, overwrite, cancellationToken);
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <param name="overwrite">If a file at the destination path exists overwrite it.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void WriteFile(string virtualPath, string contents, bool overwrite = false)
        {
            WriteFileAsync(virtualPath, contents, overwrite).Wait();
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the string contents to.</param>
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
        public async Task WriteFileAsync(string virtualPath, string contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.WriteFileAsync(virtualPath, contents, overwrite, cancellationToken);
        }

        /// <summary>
        /// Writes stream contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.AppendFileAsync(virtualPath, contents, cancellationToken);
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void AppendFile(string virtualPath, byte[] contents)
        {
            AppendFileAsync(virtualPath, contents).Wait();
        }

        /// <summary>
        /// Writes byte array contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the byte array contents to.</param>
        /// <param name="contents">The file byte array contents.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task AppendFileAsync(string virtualPath, byte[] contents, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.AppendFileAsync(virtualPath, contents, cancellationToken);
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void AppendFile(string virtualPath, string contents)
        {
            AppendFileAsync(virtualPath, contents).Wait();
        }

        /// <summary>
        /// Writes string contents to a file at the provided path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (including prefix) where to write the string contents to.</param>
        /// <param name="contents">The file string contents.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to propagate notifications that the operation should be cancelled.</param>
        /// <exception cref="ConnectionException">Thrown when an exception occurs during the adapter's connection process. Contains an inner exception with more details.</exception>
        /// <exception cref="AdapterRuntimeException">Thrown when an exception occurs during the adapter's runtime. Contains an inner exception with more details.</exception>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        /// <exception cref="Exceptions.FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public async Task AppendFileAsync(string virtualPath, string contents, CancellationToken cancellationToken = default)
        {
            var prefix = PathUtilities.GetPrefix(virtualPath);
            var adapter = GetAdapter(prefix);

            adapter.Connect();

            if (!adapter.AdapterConfiguration.EnableCache)
            {
                adapter.ClearCache();
            }

            await adapter.AppendFileAsync(virtualPath, contents, cancellationToken);
        }
    }
}