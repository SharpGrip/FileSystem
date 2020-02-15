using System;

namespace SharpGrip.FileSystem.Models
{
    public interface IDirectory
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }
}