# SharpGrip FileSystem GoogleCloudStorage adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.GoogleCloudStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleCloudStorage)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.GoogleCloudStorage` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleCloudStorage).

## Usage

```
// Google connection.
var credential = GoogleCredential.FromFile("path/to/credential/file");
var storageClient = await StorageClient.CreateAsync(credential);

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new GoogleCloudStorageAdapter(prefix, rootPath, storageClient, "bucketName", configuration);
};

var fileSystem = new FileSystem(adapters);
```