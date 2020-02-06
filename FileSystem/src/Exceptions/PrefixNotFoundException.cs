using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class PrefixNotFoundException : Exception
    {
        public PrefixNotFoundException(string message) : base(message)
        {
        }
    }
}