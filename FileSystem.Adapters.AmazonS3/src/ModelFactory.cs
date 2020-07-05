using System;
using System.Linq;
using Amazon.S3.Model;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.AmazonS3
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(GetObjectResponse file)
        {
            return new FileModel
            {
                Name = file.Key.Split('/').Last(),
                Path = file.Key,
                Length = file.ContentLength,
                LastModifiedDateTime = file.LastModified,
            };
        }

        public static FileModel CreateFile(S3Object file)
        {
            return new FileModel
            {
                Name = file.Key.Split('/').Last(),
                Path = file.Key,
                Length = file.Size,
                LastModifiedDateTime = file.LastModified
            };
        }

        public static DirectoryModel CreateDirectory(S3Object directory)
        {
            var pathParts = directory.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var name = pathParts.Last();

            if (pathParts.Length == 1)
            {
                name = directory.Key;
            }

            return new DirectoryModel
            {
                Name = name.Substring(0, name.Length - 1),
                Path = directory.Key.Substring(0, name.Length - 1),
                LastModifiedDateTime = directory.LastModified
            };
        }
    }
}