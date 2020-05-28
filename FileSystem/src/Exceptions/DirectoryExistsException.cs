namespace SharpGrip.FileSystem.Exceptions
{
    public class DirectoryExistsException : FileSystemException
    {
        public string Path { get; }
        public string Prefix { get; }

        public DirectoryExistsException(string path, string prefix) : base(GetMessage(path, prefix))
        {
            Path = path;
            Prefix = prefix;
        }

        private static string GetMessage(string path, string prefix)
        {
            return $"Directory at path '{path}' already exists in adapter with prefix '{prefix}'.";
        }
    }
}