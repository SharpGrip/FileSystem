using Google.Apis.Storage.v1.Data;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.GoogleCloudStorage
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(Object file, string path, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = path,
                VirtualPath = virtualPath,
                Length = (long?) file.Size,
                LastModifiedDateTime = file.UpdatedDateTimeOffset?.DateTime,
                CreatedDateTime = file.TimeCreatedDateTimeOffset?.DateTime
            };
        }

        public static DirectoryModel CreateDirectory(string name, string path, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = name,
                Path = path,
                VirtualPath = virtualPath
            };
        }

        public static DirectoryModel CreateDirectory(Object directory, string path, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = path,
                VirtualPath = virtualPath,
                LastModifiedDateTime = directory.UpdatedDateTimeOffset?.DateTime,
                CreatedDateTime = directory.TimeCreatedDateTimeOffset?.DateTime
            };
        }
    }
}