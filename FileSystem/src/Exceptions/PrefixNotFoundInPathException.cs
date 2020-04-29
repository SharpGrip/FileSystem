namespace SharpGrip.FileSystem.Exceptions
{
    public class PrefixNotFoundInPathException : FileSystemException
    {
        public string Path { get; }

        public PrefixNotFoundInPathException(string path) : base(GetMessage(path))
        {
            Path = path;
        }

        private static string GetMessage(string path)
        {
            return $"No prefix found in path '{path}'.";
        }
    }
}