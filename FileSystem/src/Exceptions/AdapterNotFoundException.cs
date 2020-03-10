namespace SharpGrip.FileSystem.Exceptions
{
    public class AdapterNotFoundException : FileSystemException
    {
        public AdapterNotFoundException(string prefix, string adapters) : base(GetMessage(prefix, adapters))
        {
        }

        private static string GetMessage(string prefix, string adapters)
        {
            return $"No adapter found with prefix '{prefix}'. Registered adapters are: {adapters}.";
        }
    }
}