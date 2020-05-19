using System.Collections.Generic;
using System.Linq;
using SharpGrip.FileSystem.Adapters;

namespace SharpGrip.FileSystem.Exceptions
{
    public class AdapterNotFoundException : FileSystemException
    {
        public string Prefix { get; }
        public IList<IAdapter> Adapters { get; }

        public AdapterNotFoundException(string prefix, IList<IAdapter> adapters) : base(GetMessage(prefix, adapters))
        {
            Prefix = prefix;
            Adapters = adapters;
        }

        private static string GetMessage(string prefix, IEnumerable<IAdapter> adapters)
        {
            var adaptersString = string.Join("', '", adapters.Select(adapter => adapter.Prefix).ToArray());
            
            return $"No adapter found with prefix '{prefix}'. Registered adapters are: '{adaptersString}'.";
        }
    }
}