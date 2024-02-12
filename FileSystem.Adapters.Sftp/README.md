# SharpGrip FileSystem Sftp adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Sftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Sftp)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.Sftp` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Sftp).

## Usage

```
// SFTP connection.
var privateKeyFile = new PrivateKeyFile("/home/userName/.ssh/id_rsa");
var privateKeyAuthenticationMethod = new PrivateKeyAuthenticationMethod("userName", privateKeyFile);
var sftpConnectionInfo = new ConnectionInfo("hostName", "userName", privateKeyAuthenticationMethod);
var sftpClient = new SftpClient(sftpConnectionInfo);

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new SftpAdapter("sftp", "/var/files", sftpClient)
};

var fileSystem = new FileSystem(adapters);
```