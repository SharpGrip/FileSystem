using Renci.SshNet.Sftp;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.Sftp
{
    public static class ModelFactory
    {
        public static IFile CreateFile(SftpFile file)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.FullName,
                Length = file.Length,
                LastWriteTime = file.LastWriteTime,
                LastWriteTimeUtc = file.LastWriteTimeUtc
            };
        }

        public static DirectoryModel CreateDirectory(SftpFile directory)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.FullName,
                LastWriteTime = directory.LastWriteTime,
                LastWriteTimeUtc = directory.LastWriteTimeUtc
            };
        }
    }
}