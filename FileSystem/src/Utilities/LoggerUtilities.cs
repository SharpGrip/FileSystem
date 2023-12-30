using Microsoft.Extensions.Logging;

namespace SharpGrip.FileSystem.Utilities
{
    public class LoggerUtilities
    {
        public static ILogger CreateDefaultConsoleLogger(string categoryName, LogLevel minimumLevel = LogLevel.Trace)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(minimumLevel);
            }).CreateLogger(categoryName);
        }
    }
}