# SharpGrip FileSystem

## Introduction
SharpGrip FileSystem is a file system abstraction supporting multiple adapters.

## Installation
Reference NuGet package `SharpGrip.FileSystem` (https://www.nuget.org/packages/SharpGrip.FileSystem).

For adapters other than the local file system (included in the `SharpGrip.FileSystem` package) please see the [Supported adapters](#supported-adapters) section.

## Usage
```
var privateKeyFile = new PrivateKeyFile("/home/userName/.ssh/id_rsa");
var privateKeyAuthenticationMethod = new PrivateKeyAuthenticationMethod("userName", privateKeyFile);
var sftpConnectionInfo = new ConnectionInfo("hostName", "userName", privateKeyAuthenticationMethod);

var adapters = new List<IAdapter>
{
    new LocalAdapter("uploads", Path.Combine("Path", "to", "uploads", "file", "system")),
    new LocalAdapter("archive", Path.Combine("Path", "to", "archive", "file", "system")),
    new SftpAdapter("backups", "/var/backups", sftpConnectionInfo),
};

// Option 1.
var fileSystem = new FileSystem(adapters);

// Option 2.
var fileSystem = new FileSystem();
fileSystem.Adapters = adapters;

await fileSystem.CopyFile("uploads://foo.txt", "archive://bar.txt", true);
await fileSystem.CopyFile("uploads://foo.txt", "backups://bar.txt", true);
```

## Supported adapters
- Local
- SFTP (NuGet: https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Sftp)