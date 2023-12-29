using SharpGrip.FileSystem.Configuration;

namespace SharpGrip.FileSystem.Adapters.AzureBlobStorage
{
    public class AzureBlobStorageAdapterConfiguration : AdapterConfiguration
    {
        public string DirectoryPlaceholder { get; set; } = "___sharp-grip-file-system-placeholder___";
    }
}