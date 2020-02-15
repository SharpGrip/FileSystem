using System.IO;

namespace SharpGrip.FileSystem.Models
{
    public class FileModel : Model, IFile
    {
        public FileModel()
        {
        }

        public FileModel(FileInfo file)
        {
            Name = file.Name;
            FullName = file.FullName;
            Length = file.Length;
            LastAccessTime = file.LastAccessTime;
            LastWriteTime = file.LastWriteTime;
            LastAccessTimeUtc = file.LastAccessTimeUtc;
            LastWriteTimeUtc = file.LastWriteTimeUtc;
        }
    }
}