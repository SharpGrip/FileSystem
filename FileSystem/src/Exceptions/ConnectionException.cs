using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class ConnectionException : FileSystemException
    {
        public ConnectionException(Exception innerException) : base(GetMessage(), innerException)
        {
        }

        private static string GetMessage()
        {
            return "A connection exception occured. See the inner exception for more details.";
        }
    }
}