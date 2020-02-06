using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class AdaptersNullException : Exception
    {
        public AdaptersNullException(string message) : base(message)
        {
        }
    }
}