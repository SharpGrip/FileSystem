using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class AdapterRuntimeException : FileSystemException
    {
        public AdapterRuntimeException() : base(GetMessage(null))
        {
        }

        public AdapterRuntimeException(Exception innerException) : base(GetMessage(innerException), innerException)
        {
        }

        private static string GetMessage(Exception? innerException)
        {
            if (innerException != null)
            {
                return "An adapter runtime exception occured. See the inner exception for more details.";
            }

            return "An adapter runtime exception occured.";
        }
    }
}