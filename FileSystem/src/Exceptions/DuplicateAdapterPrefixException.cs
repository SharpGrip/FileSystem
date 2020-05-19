using System.Collections.Generic;
using System.Linq;
using SharpGrip.FileSystem.Adapters;

namespace SharpGrip.FileSystem.Exceptions
{
    public class DuplicateAdapterPrefixException : FileSystemException
    {
        public DuplicateAdapterPrefixException(
            IEnumerable<IGrouping<string, IAdapter>> duplicateAdapters,
            IEnumerable<IAdapter> adapters
        ) : base(GetMessage(duplicateAdapters, adapters))
        {
        }

        private static string GetMessage(IEnumerable<IGrouping<string, IAdapter>> duplicateAdapters, IEnumerable<IAdapter> adapters)
        {
            var duplicateAdaptersString = string.Join("', '", duplicateAdapters.Select(grouping => grouping.Key).ToArray());
            var adaptersString = string.Join("', '", adapters.Select(adapter => adapter.Prefix).ToArray());

            return $"Multiple adapters registered with the prefix: '{duplicateAdaptersString}'. Registered adapters are: '{adaptersString}'.";
        }
    }
}