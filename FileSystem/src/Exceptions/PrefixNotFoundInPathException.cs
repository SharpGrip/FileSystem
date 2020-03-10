namespace SharpGrip.FileSystem.Exceptions
{
    public class PrefixNotFoundInPathException : FileSystemException
    {
        public PrefixNotFoundInPathException(string path) : base(GetMessage(path))
        {
        }

        private static string GetMessage(string path)
        {
            return $"No prefix found in path '{path}'.";
        }
    }
}