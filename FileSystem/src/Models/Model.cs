using System;

namespace SharpGrip.FileSystem.Models
{
    public abstract class Model
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public DateTime LastWriteTime {get; set; }
        public DateTime LastWriteTimeUtc {get; set; }
    }
}