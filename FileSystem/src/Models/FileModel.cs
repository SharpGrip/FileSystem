using System.IO;

namespace SharpGrip.FileSystem.Models
{
    public class FileModel : Model, IFile
    {
        public long? Length { get; set; }

        public FileModel()
        {
        }

        public FileModel(FileInfo file)
        {
            Name = file.Name;
            Path = file.FullName;
            Length = file.Length;
            LastModifiedDateTime = file.LastWriteTime;
            CreatedDateTime = file.CreationTime;
        }
    }
}