using Google.Apis.Drive.v3.Data;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.GoogleDrive
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(File file, string path, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = path,
                VirtualPath = virtualPath,
                Length = file.Size,
                LastModifiedDateTime = file.ModifiedByMeTimeDateTimeOffset?.DateTime,
                CreatedDateTime = file.CreatedTimeDateTimeOffset?.DateTime
            };
        }

        public static DirectoryModel CreateDirectory(File directory, string path, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = path,
                VirtualPath = virtualPath,
                LastModifiedDateTime = directory.ModifiedByMeTimeDateTimeOffset?.DateTime,
                CreatedDateTime = directory.CreatedTimeDateTimeOffset?.DateTime
            };
        }
    }
}