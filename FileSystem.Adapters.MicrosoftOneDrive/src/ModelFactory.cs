using Microsoft.Graph;
using SharpGrip.FileSystem.Models;

namespace SharpGrip.FileSystem.Adapters.MicrosoftOneDrive
{
    public static class ModelFactory
    {
        public static FileModel CreateFile(DriveItem file, string path)
        {
            return new FileModel
            {
                Name = file.Name,
                Path = path,
                Length = file.Size ?? 0,
                LastModifiedDateTime = file.LastModifiedDateTime?.DateTime,
                CreatedDateTime = file.CreatedDateTime?.DateTime
            };
        }

        public static DirectoryModel CreateDirectory(DriveItem directory, string path)
        {
            var name = directory.Name;

            // Override the root folder name with a blank one.
            if (name == "root" && path == "")
            {
                name = "";
            }
            
            return new DirectoryModel
            {
                Name = name,
                Path = path,
                LastModifiedDateTime = directory.LastModifiedDateTime?.DateTime,
                CreatedDateTime = directory.CreatedDateTime?.DateTime
            };
        }
    }
}