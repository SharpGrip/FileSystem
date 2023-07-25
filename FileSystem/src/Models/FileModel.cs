namespace SharpGrip.FileSystem.Models
{
    public class FileModel : Model, IFile
    {
        public long? Length { get; set; }
    }
}