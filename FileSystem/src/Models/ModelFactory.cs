using System.IO;

namespace SharpGrip.FileSystem.Models
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(FileInfo file, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.FullName,
                VirtualPath = virtualPath,
                Length = file.Length,
                LastModifiedDateTime = file.LastWriteTime,
                CreatedDateTime = file.CreationTime
            };
        }

        public static DirectoryModel CreateDirectory(DirectoryInfo directory, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.FullName,
                VirtualPath = virtualPath,
                LastModifiedDateTime = directory.LastWriteTime,
                CreatedDateTime = directory.CreationTime
            };
        }
    }
}