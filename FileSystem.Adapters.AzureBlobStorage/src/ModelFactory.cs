using Azure.Storage.Blobs.Models;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.AzureBlobStorage
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(BlobItem file)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.Name,
                Length = file.Properties.ContentLength,
                LastModifiedDateTime = file.Properties.LastModified?.DateTime,
                CreatedDateTime = file.Properties.CreatedOn?.DateTime
            };
        }

        public static DirectoryModel CreateDirectory(string name, string path)
        {
            return new DirectoryModel
            {
                Name = name,
                Path = path
            };
        }
    }
}