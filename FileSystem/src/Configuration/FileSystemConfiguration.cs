using Microsoft.Extensions.Logging;

namespace SharpGrip.FileSystem.Configuration
{
    public class FileSystemConfiguration : IConfiguration
    {
        public bool EnableLogging { get; set; }
        public ILogger? Logger { get; set; }
    }
}