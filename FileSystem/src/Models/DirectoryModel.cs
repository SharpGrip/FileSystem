using System.IO;

namespace SharpGrip.FileSystem.Models
{
    public class DirectoryModel : Model, IDirectory
    {
        public DirectoryModel()
        {
        }

        public DirectoryModel(DirectoryInfo directory)
        {
            Name = directory.Name;
            FullName = directory.FullName;
            LastAccessTime = directory.LastAccessTime;
            LastWriteTime = directory.LastWriteTime;
            LastAccessTimeUtc = directory.LastAccessTimeUtc;
            LastWriteTimeUtc = directory.LastWriteTimeUtc;
        }
    }
}