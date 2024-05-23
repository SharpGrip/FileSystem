# SharpGrip FileSystem Dropbox adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Dropbox)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Dropbox)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.Dropbox` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Dropbox).

## Usage

```
// Dropbox connection.
var dropboxClient = new DropboxClient("oAuth2AccessToken");

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new DropboxAdapter("dropbox", "/Files", dropboxClient)
};

var fileSystem = new FileSystem(adapters);
```