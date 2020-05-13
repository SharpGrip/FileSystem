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
                LastWriteTime = file.LastModified,
                LastWriteTimeUtc = file.LastModified.ToUniversalTime()
            };
        }

        public static FileModel CreateFile(S3Object file)
        {
            return new FileModel
            {
                Name = file.Key.Split('/').Last(),
                Path = file.Key,
                Length = file.Size,
                LastWriteTime = file.LastModified,
                LastWriteTimeUtc = file.LastModified.ToUniversalTime()
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
                LastWriteTime = directory.LastModified,
                LastWriteTimeUtc = directory.LastModified.ToUniversalTime()
            };
        }
    }
}