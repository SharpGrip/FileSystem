﻿using Microsoft.Extensions.Logging;

namespace SharpGrip.FileSystem.Configuration
{
    public abstract class AdapterConfiguration : IAdapterConfiguration
    {
        public bool EnableCache { get; set; }
        public bool EnableLogging { get; set; }
        public ILogger? Logger { get; set; }
    }
}