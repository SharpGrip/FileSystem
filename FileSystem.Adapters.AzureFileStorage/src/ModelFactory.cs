using Azure.Storage.Files.Shares;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.AzureFileStorage
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(ShareFileClient file)
        {
            var fileProperties = file.GetProperties().Value;

            return new FileModel
            {
                Name = file.Name,
                Path = file.Path,
                Length = fileProperties.ContentLength,
                LastModifiedDateTime = fileProperties.LastModified.DateTime,
            };
        }

        public static DirectoryModel CreateDirectory(ShareDirectoryClient directory)
        {
            var directoryProperties = directory.GetProperties().Value;

            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.Path,
                LastModifiedDateTime = directoryProperties.LastModified.DateTime
            };
        }
    }
}