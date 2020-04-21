namespace SharpGrip.FileSystem.Exceptions
{
    public class FileNotFoundException : FileSystemException
    {
        public FileNotFoundException(string path, string prefix) : base(GetMessage(path, prefix))
        {
        }

        private static string GetMessage(string path, string prefix)
        {
            return $"File '{path}' not found in adapter with prefix '{prefix}'.";
        }
    }
}