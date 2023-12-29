using Microsoft.Extensions.Logging;

namespace SharpGrip.FileSystem.Configuration
{
    public interface IConfiguration
    {
        public bool EnableLogging { get; set; }
        public ILogger? Logger { get; set; }
    }
}