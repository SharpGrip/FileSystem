namespace SharpGrip.FileSystem.Exceptions
{
    public class AdapterNotFoundException : FileSystemException
    {
        public string Prefix { get; }
        public string Adapters { get; }

        public AdapterNotFoundException(string prefix, string adapters) : base(GetMessage(prefix, adapters))
        {
            Prefix = prefix;
            Adapters = adapters;
        }

        private static string GetMessage(string prefix, string adapters)
        {
            return $"No adapter found with prefix '{prefix}'. Registered adapters are: {adapters}.";
        }
    }
}