# SharpGrip FileSystem AmazonS3 adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AmazonS3)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AmazonS3)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.AmazonS3` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AmazonS3).

## Usage

```
// Amazon connection.
var amazonClient = new AmazonS3Client("awsAccessKeyId", "awsSecretAccessKey", RegionEndpoint.USEast2);

var adapters = new List<IAdapter>
{
    new AmazonS3Adapter("amazon1", "/Files", amazonClient, "bucketName1")
    new AmazonS3Adapter("amazon2", "/Files", amazonClient, "bucketName2")
};

var fileSystem = new FileSystem(adapters);
```