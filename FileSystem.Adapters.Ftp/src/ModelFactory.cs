using FluentFTP;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.Ftp
{
    public static class ModelFactory
    {
        public static IFile CreateFile(FtpListItem file, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.FullName,
                VirtualPath = virtualPath,
                Length = file.Size,
                LastModifiedDateTime = file.Modified,
                CreatedDateTime = file.Created
            };
        }

        public static DirectoryModel CreateDirectory(FtpListItem directory, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.FullName,
                VirtualPath = virtualPath,
                LastModifiedDateTime = directory.Modified,
                CreatedDateTime = directory.Created
            };
        }
    }
}