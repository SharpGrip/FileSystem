using Dropbox.Api.Files;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.Dropbox
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(Metadata file, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.PathDisplay,
                VirtualPath = virtualPath,
                Length = (long) file.AsFile.Size,
                LastModifiedDateTime = file.AsFile.ServerModified
            };
        }

        public static DirectoryModel CreateDirectory(Metadata directory, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.PathDisplay,
                VirtualPath = virtualPath
            };
        }
    }
}