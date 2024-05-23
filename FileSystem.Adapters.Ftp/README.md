# SharpGrip FileSystem Ftp adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Ftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Ftp)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.Ftp` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Ftp).

## Usage

```
// FTP connection.
var ftpClient = new AsyncFtpClient("hostname", "username", "password");

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new FtpAdapter("ftp", "/var/files", ftpClient)
};

var fileSystem = new FileSystem(adapters);
```