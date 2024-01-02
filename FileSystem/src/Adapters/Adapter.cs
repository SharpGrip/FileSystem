using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharpGrip.FileSystem.Cache;
using SharpGrip.FileSystem.Configuration;
using SharpGrip.FileSystem.Constants;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters
{
    public abstract class Adapter<TAdapterConfiguration, TCacheKey, TCacheValue> : IAdapter where TAdapterConfiguration : IAdapterConfiguration, new() where TCacheKey : class
    {
        public string Prefix { get; }
        public string RootPath { get; }
        public string Name { get; }
        public IAdapterConfiguration AdapterConfiguration => Configuration;
        protected TAdapterConfiguration Configuration { get; }
        protected ILogger Logger { get; } = NullLogger<Adapter<TAdapterConfiguration, TCacheKey, TCacheValue>>.Instance;

        private IDictionary<TCacheKey, CacheEntry<TCacheKey, TCacheValue>> Cache { get; set; } = new Dictionary<TCacheKey, CacheEntry<TCacheKey, TCacheValue>>();

        protected Adapter(string prefix, string rootPath, Action<TAdapterConfiguration>? configuration)
        {
            var adapterConfiguration = new TAdapterConfiguration();
            configuration?.Invoke(adapterConfiguration);

            Name = GetType().FullName!;
            Configuration = adapterConfiguration;

            if (Configuration.EnableLogging)
            {
                Logger = Configuration.Logger ?? LoggerUtilities.CreateDefaultConsoleLogger(Name);
            }

            Logger.LogStartNormalizingRootPath(rootPath);

            Prefix = prefix;
            RootPath = PathUtilities.NormalizeRootPath(rootPath);

            Logger.LogFinishedNormalizingRootPath(rootPath, RootPath);
        }

        public async Task<bool> FileExistsAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogStartExecutingMethod(nameof(FileExistsAsync));
                await GetFileAsync(virtualPath, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            finally
            {
                Logger.LogFinishedExecutingMethod(nameof(FileExistsAsync));
            }

            return true;
        }

        public async Task<bool> DirectoryExistsAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogStartExecutingMethod(nameof(DirectoryExistsAsync));
                await GetDirectoryAsync(virtualPath, cancellationToken);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            finally
            {
                Logger.LogFinishedExecutingMethod(nameof(DirectoryExistsAsync));
            }

            return true;
        }

        public virtual async Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default)
        {
            Logger.LogStartExecutingMethod(nameof(AppendFileAsync));

            await GetFileAsync(virtualPath, cancellationToken);

            using var fileContents = await ReadFileStreamAsync(virtualPath, cancellationToken);
            fileContents.Seek(0, SeekOrigin.Begin);

            using var memoryStream = await StreamUtilities.CopyContentsToMemoryStreamAsync(fileContents, false, cancellationToken);

            await contents.CopyToAsync(memoryStream, FileSystemConstants.Streaming.DefaultMemoryStreamBufferSize, cancellationToken);
            memoryStream.Seek(0, SeekOrigin.Begin);

            await DeleteFileAsync(virtualPath, cancellationToken);

            try
            {
                await WriteFileAsync(virtualPath, memoryStream, true, cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
            finally
            {
                Logger.LogFinishedExecutingMethod(nameof(AppendFileAsync));
            }
        }

        public abstract void Dispose();
        public abstract Task ConnectAsync(CancellationToken cancellationToken = default);
        public abstract Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default);
        public abstract Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default);
        public abstract Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default);
        protected abstract Exception Exception(Exception exception);

        protected string GetPath(string path)
        {
            return PathUtilities.GetPath(path, RootPath);
        }

        protected string GetVirtualPath(string path)
        {
            return PathUtilities.GetVirtualPath(path, Prefix, RootPath);
        }

        protected string GetLastPathPart(string path)
        {
            return PathUtilities.GetLastPathPart(path);
        }

        protected string GetParentPathPart(string path)
        {
            return PathUtilities.GetParentPathPart(path);
        }

        public void ClearCache()
        {
            Logger.LogStartClearingAdapterCache(this);
            Cache = new Dictionary<TCacheKey, CacheEntry<TCacheKey, TCacheValue>>();
            Logger.LogFinishedClearingAdapterCache(this);
        }

        protected async Task<CacheEntry<TCacheKey, TCacheValue>> GetOrCreateCacheEntryAsync(TCacheKey cacheKey, Func<Task<CacheEntry<TCacheKey, TCacheValue>>> factory)
        {
            var cacheEntry = GetCacheEntry(cacheKey);

            if (cacheEntry == null)
            {
                cacheEntry = await factory();

                TryAddCacheEntry(cacheEntry);
            }

            return cacheEntry;
        }

        protected void TryAddCacheEntry(CacheEntry<TCacheKey, TCacheValue> cacheEntry)
        {
            if (Configuration.EnableCache && !Cache.ContainsKey(cacheEntry.Key))
            {
                Logger.LogStartAddingEntryToAdapterCache(this, cacheEntry.Key);
                Cache.Add(cacheEntry.Key, cacheEntry);
                Logger.LogFinishedAddingEntryToAdapterCache(this, cacheEntry.Key);
            }
        }

        protected void TryRemoveCacheEntry(TCacheKey cacheKey)
        {
            if (Configuration.EnableCache)
            {
                Logger.LogStartRemovingEntryFromAdapterCache(this, cacheKey);
                Cache.Remove(cacheKey);
                Logger.LogFinishedRemovingEntryFromAdapterCache(this, cacheKey);
            }
        }

        private CacheEntry<TCacheKey, TCacheValue>? GetCacheEntry(TCacheKey cacheKey)
        {
            if (Configuration.EnableCache)
            {
                try
                {
                    Logger.LogStartRetrievingEntryFromAdapterCache(this, cacheKey);
                    return Cache.TryGetValue(cacheKey, out var cacheEntry) ? cacheEntry : null;
                }
                finally
                {
                    Logger.LogFinishedRetrievingEntryFromAdapterCache(this, cacheKey);
                }
            }

            return null;
        }
    }
}