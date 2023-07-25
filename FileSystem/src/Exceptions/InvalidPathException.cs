namespace SharpGrip.FileSystem.Exceptions
{
    public class InvalidPathException : FileSystemException
    {
        public string Path { get; }

        public InvalidPathException(string path) : base(GetMessage(path))
        {
            Path = path;
        }

        private static string GetMessage(string path)
        {
            return $"Invalid path '{path}'.";
        }
    }
}