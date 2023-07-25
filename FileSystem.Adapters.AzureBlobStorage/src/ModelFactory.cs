using System.Linq;
using Azure.Storage.Blobs.Models;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.AzureBlobStorage
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(BlobItem file, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name.Split('/').Last(),
                Path = file.Name,
                VirtualPath = virtualPath,
                Length = file.Properties.ContentLength,
                LastModifiedDateTime = file.Properties.LastModified?.DateTime,
                CreatedDateTime = file.Properties.CreatedOn?.DateTime
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
    }
}