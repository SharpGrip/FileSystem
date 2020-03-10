namespace SharpGrip.FileSystem.Exceptions
{
    public class FileExistsException : FileSystemException
    {
        public FileExistsException(string path, string prefix) : base(GetMessage(path, prefix))
        {
        }

        private static string GetMessage(string path, string prefix)
        {
            return $"File at path '{path}' already exists in adapter with prefix '{prefix}'.";
        }
    }
}