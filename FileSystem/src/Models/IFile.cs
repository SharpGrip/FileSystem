using System;

namespace SharpGrip.FileSystem.Models
{
    public interface IFile
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public long Length { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }
}