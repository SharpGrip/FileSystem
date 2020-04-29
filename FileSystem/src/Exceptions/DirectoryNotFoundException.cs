namespace SharpGrip.FileSystem.Exceptions
{
    public class DirectoryNotFoundException : FileSystemException
    {
        public string Path { get; }
        public string Prefix { get; }

        public DirectoryNotFoundException(string path, string prefix) : base(GetMessage(path, prefix))
        {
            Path = path;
            Prefix = prefix;
        }

        private static string GetMessage(string path, string prefix)
        {
            return $"Directory '{path}' not found in adapter with prefix '{prefix}'.";
        }
    }
}