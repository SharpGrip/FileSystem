using Microsoft.Extensions.Logging;
using SharpGrip.FileSystem.Adapters;

namespace SharpGrip.FileSystem.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogStartNormalizingRootPath(this ILogger logger, string rootPath)
        {
            logger.LogTrace("Start normalizing root path '{RootPath}'", rootPath);
        }

        public static void LogFinishedNormalizingRootPath(this ILogger logger, string oldRootPath, string newRootPath)
        {
            logger.LogTrace("Finished normalizing root path from '{OldRootPath}' to '{NewRootPath}'", oldRootPath, newRootPath);
        }

        public static void LogStartDisposingAdapters(this ILogger logger, int adapterCount)
        {
            logger.LogTrace("Start disposing {AdapterCount} adapters", adapterCount);
        }

        public static void LogFinishedDisposingAdapters(this ILogger logger, int adapterCount)
        {
            logger.LogTrace("Finished disposing {AdapterCount} adapters", adapterCount);
        }

        public static void LogStartDisposingAdapter(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Start disposing adapter '{Name}' with prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogFinishedDisposingAdapter(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Finished disposing adapter '{Name}' with prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogStartRetrievingAdapter(this ILogger logger, string adapterPrefix)
        {
            logger.LogTrace("Start retrieving adapter for prefix '{Prefix}'", adapterPrefix);
        }

        public static void LogFinishedRetrievingAdapter(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Finished retrieving adapter '{Name}' for prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogStartConnectingAdapter(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Start connecting to adapter '{Name}' with prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogFinishedConnectingAdapter(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Finished connecting to adapter '{Name}' with prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogStartClearingAdapterCache(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Start clearing cache of adapter '{Name}' with prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogFinishedClearingAdapterCache(this ILogger logger, IAdapter adapter)
        {
            logger.LogTrace("Finished clearing cache of adapter '{Name}' with prefix '{Prefix}'", adapter.Name, adapter.Prefix);
        }

        public static void LogStartAddingEntryToAdapterCache<TCacheKey>(this ILogger logger, IAdapter adapter, TCacheKey cacheKey)
        {
            logger.LogTrace("Start adding entry with cache key '{CacheKey}' to cache of adapter '{Name}' with prefix '{Prefix}'", cacheKey, adapter.Name, adapter.Prefix);
        }

        public static void LogFinishedAddingEntryToAdapterCache<TCacheKey>(this ILogger logger, IAdapter adapter, TCacheKey cacheKey)
        {
            logger.LogTrace("Finished adding entry with cache key '{CacheKey}' to cache of adapter '{Name}' with prefix '{Prefix}'", cacheKey, adapter.Name, adapter.Prefix);
        }

        public static void LogStartRetrievingEntryFromAdapterCache<TCacheKey>(this ILogger logger, IAdapter adapter, TCacheKey cacheKey)
        {
            logger.LogTrace("Start retrieving entry with cache key '{CacheKey}' from cache of adapter '{Name}' with prefix '{Prefix}'", cacheKey, adapter.Name, adapter.Prefix);
        }

        public static void LogFinishedRetrievingEntryFromAdapterCache<TCacheKey>(this ILogger logger, IAdapter adapter, TCacheKey cacheKey)
        {
            logger.LogTrace("Finished retrieving entry with cache key '{CacheKey}' from cache of adapter '{Name}' with prefix '{Prefix}'", cacheKey, adapter.Name, adapter.Prefix);
        }

        public static void LogStartRemovingEntryFromAdapterCache<TCacheKey>(this ILogger logger, IAdapter adapter, TCacheKey cacheKey)
        {
            logger.LogTrace("Start removing entry with cache key '{CacheKey}' from cache of adapter '{Name}' with prefix '{Prefix}'", cacheKey, adapter.Name, adapter.Prefix);
        }

        public static void LogFinishedRemovingEntryFromAdapterCache<TCacheKey>(this ILogger logger, IAdapter adapter, TCacheKey cacheKey)
        {
            logger.LogTrace("Finished removing entry with cache key '{CacheKey}' from cache of adapter '{Name}' with prefix '{Prefix}'", cacheKey, adapter.Name, adapter.Prefix);
        }

        public static void LogStartExecutingMethod(this ILogger logger, string methodName)
        {
            logger.LogDebug("Start executing method '{MethodName}'", methodName);
        }

        public static void LogFinishedExecutingMethod(this ILogger logger, string methodName)
        {
            logger.LogDebug("Finished executing method '{MethodName}'", methodName);
        }
    }
}