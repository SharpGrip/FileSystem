namespace SharpGrip.FileSystem.Exceptions
{
    public class NoAdaptersRegisteredException : FileSystemException
    {
        public NoAdaptersRegisteredException() : base(GetMessage())
        {
        }

        private static string GetMessage()
        {
            return "No adapters registered with the file system.";
        }
    }
}