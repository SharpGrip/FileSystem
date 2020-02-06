using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class AdaptersEmptyException : Exception
    {
        public AdaptersEmptyException(string message) : base(message)
        {
        }
    }
}