using Microsoft.Extensions.Logging;
using SharpGrip.FileSystem.Adapters;

namespace SharpGrip.FileSystem.Extensions
{
    public static class LoggerExtensions
    {
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
    }
}