using Dropbox.Api.Files;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.Dropbox
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(Metadata file)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = file.PathDisplay,
                Length = (long) file.AsFile.Size,
                LastWriteTime = file.AsFile.ServerModified,
                LastWriteTimeUtc = file.AsFile.ServerModified.ToUniversalTime()
            };
        }

        public static DirectoryModel CreateDirectory(Metadata directory)
        {
            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.PathDisplay
            };
        }
    }
}