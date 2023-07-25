using Azure.Storage.Files.Shares;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.AzureFileStorage
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(ShareFileClient file, string virtualPath)
        {
            var fileProperties = file.GetProperties().Value;

            return new FileModel
            {
                Name = file.Name,
                Path = file.Path,
                VirtualPath = virtualPath,
                Length = fileProperties.ContentLength,
                LastModifiedDateTime = fileProperties.LastModified.DateTime,
            };
        }

        public static DirectoryModel CreateDirectory(ShareDirectoryClient directory, string virtualPath)
        {
            var directoryProperties = directory.GetProperties().Value;

            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.Path,
                VirtualPath = virtualPath,
                LastModifiedDateTime = directoryProperties.LastModified.DateTime
            };
        }
    }
}