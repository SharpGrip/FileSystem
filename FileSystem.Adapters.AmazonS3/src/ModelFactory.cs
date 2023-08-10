using System;
using System.Linq;
using Amazon.S3.Model;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.AmazonS3
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(GetObjectResponse file, string path, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Key.Split('/').Last(),
                Path = path,
                VirtualPath = virtualPath,
                Length = file.ContentLength,
                LastModifiedDateTime = file.LastModified
            };
        }

        public static FileModel CreateFile(S3Object file, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Key.Split('/').Last(),
                Path = file.Key,
                VirtualPath = virtualPath,
                Length = file.Size,
                LastModifiedDateTime = file.LastModified
            };
        }

        public static DirectoryModel CreateDirectory(S3Object directory, string virtualPath)
        {
            var pathParts = directory.Key.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var name = pathParts.LastOrDefault();

            if (name == null)
            {
                name = "/";
            }

            if (pathParts.Length == 1)
            {
                name = directory.Key;
            }

            return new DirectoryModel
            {
                Name = name,
                Path = directory.Key.RemoveTrailingForwardSlash(),
                VirtualPath = virtualPath,
                LastModifiedDateTime = directory.LastModified
            };
        }
    }
}