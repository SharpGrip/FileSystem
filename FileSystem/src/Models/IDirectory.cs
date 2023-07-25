using System;

namespace SharpGrip.FileSystem.Models
{
    public interface IDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string VirtualPath { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }
}