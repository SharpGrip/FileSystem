using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem
{
    public interface IFileSystem
    {
        /// <summary>
        /// The adapters.
        /// </summary>
        public IList<IAdapter> Adapters { get; set; }

        /// <summary>
        /// Returns an adapter by prefix.
        /// </summary>
        /// <param name="prefix">The adapter's prefix.</param>
        /// <returns>The adapter.</returns>
        /// <exception cref="NoAdaptersRegisteredException">Thrown when no adapters are registered with the file system.</exception>
        /// <exception cref="DuplicateAdapterPrefixException">Thrown when multiple adapters are registered with the same prefix.</exception>
        /// <exception cref="AdapterNotFoundException">Thrown when an adapter could not be found via the provided prefix.</exception>
        /// <exception cref="PrefixNotFoundInPathException">Thrown when a prefix in the provided path could not be found.</exception>
        public IAdapter GetAdapter(string prefix);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public IFile GetFile(string virtualPath);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IDirectory GetDirectory(string virtualPath);

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IEnumerable<IFile> GetFiles(string virtualPath = "");

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default);

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public IEnumerable<IDirectory> GetDirectories(string virtualPath = "");

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default);

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
        public bool FileExists(string virtualPath);

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
        public Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        public bool DirectoryExists(string virtualPath);

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
        public Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        public void CreateDirectory(string virtualPath);

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
        public Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void DeleteFile(string virtualPath);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public void DeleteDirectory(string virtualPath);

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
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exists at the given path.</exception>
        public Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public byte[] ReadFile(string virtualPath);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task<byte[]> ReadFileAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public string ReadTextFile(string virtualPath);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task<string> ReadTextFileAsync(string virtualPath, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void CopyFile(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public Task CopyFileAsync(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public void MoveFile(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        /// <exception cref="FileExistsException">Thrown if the file exists at the given path and parameter "overwrite" = false.</exception>
        public Task MoveFileAsync(string virtualSourcePath, string virtualDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

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
        public void WriteFile(string virtualPath, byte[] contents, bool overwrite = false);

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
        public Task WriteFileAsync(string virtualPath, byte[] contents, bool overwrite = false, CancellationToken cancellationToken = default);

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
        public void WriteFile(string virtualPath, string contents, bool overwrite = false);

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
        public Task WriteFileAsync(string virtualPath, string contents, bool overwrite = false, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void AppendFile(string virtualPath, byte[] contents);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task AppendFileAsync(string virtualPath, byte[] contents, CancellationToken cancellationToken = default);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public void AppendFile(string virtualPath, string contents);

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
        /// <exception cref="FileNotFoundException">Thrown if the file does not exists at the given path.</exception>
        public Task AppendFileAsync(string virtualPath, string contents, CancellationToken cancellationToken = default);
    }
}