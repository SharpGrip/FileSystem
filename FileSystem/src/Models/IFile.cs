using System;

namespace SharpGrip.FileSystem.Models
{
    public interface IFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Length { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }
}