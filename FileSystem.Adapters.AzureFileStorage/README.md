# SharpGrip FileSystem AzureFileStorage adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AzureFileStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureFileStorage)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.AzureFileStorage` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureFileStorage).

## Usage

```
// Azure connection.
var azureClient = new ShareClient("connectionString", "shareName");

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new AzureFileStorageAdapter("azure", "/Files", azureClient)
};

var fileSystem = new FileSystem(adapters);
```