namespace SharpGrip.FileSystem.Configuration
{
    public interface IAdapterConfiguration : IConfiguration
    {
        public bool EnableCache { get; set; }
    }
}