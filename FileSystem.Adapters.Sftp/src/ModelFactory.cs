using Renci.SshNet.Sftp;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.Sftp
{
    public static class ModelFactory
    {
        public static IFile CreateFile(SftpFile file, string virtualPath)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.FullName,
                VirtualPath = virtualPath,
                Length = file.Length,
                LastModifiedDateTime = file.LastWriteTime
            };
        }

        public static DirectoryModel CreateDirectory(SftpFile directory, string virtualPath)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.FullName,
                VirtualPath = virtualPath,
                LastModifiedDateTime = directory.LastWriteTime
            };
        }
    }
}