namespace SharpGrip.FileSystem.Exceptions
{
    public class FileNotFoundException : FileSystemException
    {
        public string Path { get; }
        public string Prefix { get; }

        public FileNotFoundException(string path, string prefix) : base(GetMessage(path, prefix))
        {
            Path = path;
            Prefix = prefix;
        }

        private static string GetMessage(string path, string prefix)
        {
            return $"File '{path}' not found in adapter with prefix '{prefix}'.";
        }
    }
}