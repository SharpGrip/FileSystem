namespace SharpGrip.FileSystem.Exceptions
{
    public class InvalidVirtualPathException : FileSystemException
    {
        public string VirtualPath { get; }

        public InvalidVirtualPathException(string virtualPath) : base(GetMessage(virtualPath))
        {
            VirtualPath = virtualPath;
        }

        private static string GetMessage(string virtualPath)
        {
            return $"Invalid virtual path '{virtualPath}'.";
        }
    }
}