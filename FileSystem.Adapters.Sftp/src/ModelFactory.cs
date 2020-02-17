using Renci.SshNet.Sftp;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.Sftp
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(SftpFile file)
        {
            return new FileModel
            {
                Name = file.Name,
                FullName = file.FullName,
                Length = file.Length,
                LastAccessTime = file.LastAccessTime,
                LastWriteTime = file.LastWriteTime,
                LastAccessTimeUtc = file.LastAccessTimeUtc,
                LastWriteTimeUtc = file.LastWriteTimeUtc
            };
        }

        public static DirectoryModel CreateDirectory(SftpFile directory)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                FullName = directory.FullName,
                LastAccessTime = directory.LastAccessTime,
                LastWriteTime = directory.LastWriteTime,
                LastAccessTimeUtc = directory.LastAccessTimeUtc,
                LastWriteTimeUtc = directory.LastWriteTimeUtc
            };
        }
    }
}