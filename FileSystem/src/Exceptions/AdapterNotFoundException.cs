using System;

namespace SharpGrip.FileSystem.Exceptions
{
    public class AdapterNotFoundException : Exception
    {
        public AdapterNotFoundException(string message) : base(message)
        {
            
        }
    }
}