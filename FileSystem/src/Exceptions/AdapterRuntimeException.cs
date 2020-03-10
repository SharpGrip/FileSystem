using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class AdapterRuntimeException : FileSystemException
    {
        public AdapterRuntimeException(Exception innerException) : base(GetMessage(), innerException)
        {
        }

        private static string GetMessage()
        {
            return "An adapter runtime exception occured. See the inner exception for more details.";
        }
    }
}