using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class FileExistsException : Exception
    {
        public FileExistsException(string message) : base(message)
        {
        }
    }
}