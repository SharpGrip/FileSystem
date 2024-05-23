# SharpGrip FileSystem AzureBlobStorage adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AzureBlobStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureBlobStorage)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.AzureBlobStorage` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureBlobStorage).

## Usage

```
// Azure connection.
var blobServiceClient = new BlobServiceClient("connectionString");
var azureClient = blobServiceClient.GetBlobContainerClient("blobContainerName");

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new AzureBlobStorageAdapter("azure", "/Files", azureClient)
};

var fileSystem = new FileSystem(adapters);
```