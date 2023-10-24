namespace SharpGrip.FileSystem.Configuration
{
    public abstract class AdapterConfiguration : IAdapterConfiguration
    {
        public bool EnableCache { get; set; }
    }
}