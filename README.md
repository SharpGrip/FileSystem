# SharpGrip FileSystem [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem)](https://www.nuget.org/packages/SharpGrip.FileSystem)

## Builds

[![FileSystem [Build]](https://github.com/SharpGrip/FileSystem/actions/workflows/Build.yaml/badge.svg)](https://github.com/SharpGrip/FileSystem/actions/workflows/Build.yaml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=alert_status)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=sqale_rating)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=reliability_rating)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=security_rating)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=coverage)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem)

## Introduction

SharpGrip FileSystem is a versatile .NET file system abstraction that supports multiple storage adapters.
It empowers developers to manage various file systems and services through a unified and easily comprehensible API.
By coding against the abstractions provided by this library, developers can sidestep vendor-specific APIs, effectively avoiding vendor lock-ins.
This flexibility enhances the portability and maintainability of the codebase, allowing for smoother transitions between different file systems.

## Installation

Reference NuGet package `SharpGrip.FileSystem` (https://www.nuget.org/packages/SharpGrip.FileSystem).

For adapters other than the local file system (included in the `SharpGrip.FileSystem` package) please see the [Supported adapters](#supported-adapters) section.

## Supported adapters

| Adapter                                                                | Package                                            | NuGet                                                                                                                                                                                                                                                                                                                                                      |
|:-----------------------------------------------------------------------|:---------------------------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Local adapter](#local-adapter)                                        | `SharpGrip.FileSystem`                             | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem)](https://www.nuget.org/packages/SharpGrip.FileSystem) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem)](https://www.nuget.org/packages/SharpGrip.FileSystem)                                                                                                                 |
| [AmazonS3](FileSystem.Adapters.AmazonS3/README.md)                     | `SharpGrip.FileSystem.Adapters.AmazonS3`           | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AmazonS3)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AmazonS3) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.AmazonS3)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AmazonS3)                                         |
| [AzureBlobStorage](FileSystem.Adapters.AzureBlobStorage/README.md)     | `SharpGrip.FileSystem.Adapters.AzureBlobStorage`   | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AzureBlobStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureBlobStorage) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.AzureBlobStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureBlobStorage)         |
| [AzureFileStorage](FileSystem.Adapters.AzureFileStorage/README.md)     | `SharpGrip.FileSystem.Adapters.AzureFileStorage`   | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AzureFileStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureFileStorage) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.AzureFileStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureFileStorage)         |
| [Dropbox](FileSystem.Adapters.Dropbox/README.md)                       | `SharpGrip.FileSystem.Adapters.Dropbox`            | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Dropbox)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Dropbox) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.Dropbox)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Dropbox)                                             |
| [FTP](FileSystem.Adapters.Ftp/README.md)                               | `SharpGrip.FileSystem.Adapters.Ftp`                | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Ftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Ftp) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.Ftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Ftp)                                                             |
| [GoogleCloudStorage](FileSystem.Adapters.GoogleCloudStorage/README.md) | `SharpGrip.FileSystem.Adapters.GoogleCloudStorage` | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.GoogleCloudStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleCloudStorage) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.GoogleCloudStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleCloudStorage) |
| [GoogleDrive](FileSystem.Adapters.GoogleDrive/README.md)               | `SharpGrip.FileSystem.Adapters.GoogleDrive`        | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.GoogleDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleDrive) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.GoogleDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleDrive)                             |
| [MicrosoftOneDrive](FileSystem.Adapters.MicrosoftOneDrive/README.md)   | `SharpGrip.FileSystem.Adapters.MicrosoftOneDrive`  | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive)     |
| [SFTP](FileSystem.Adapters.Sftp/README.md)                             | `SharpGrip.FileSystem.Adapters.Sftp`               | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Sftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Sftp) [![NuGet](https://img.shields.io/nuget/dt/SharpGrip.FileSystem.Adapters.Sftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Sftp)                                                         |

## Supported operations

For a full list of the supported operations please see the [IFileSystem](../master/FileSystem/src/IFileSystem.cs) interface.

## Usage

### Instantiation

```
var adapters = new List<IAdapter>
{
    new LocalAdapter("adapterPrefix", "adapterRootPath")
};

// Instantiation option 1.
var fileSystem = new FileSystem(adapters);

// Instantiation option 2.
var fileSystem = new FileSystem();
fileSystem.Adapters = adapters;
```

### Local adapter

```
var adapters = new List<IAdapter>
{
    new LocalAdapter("local1", "/var/files"),
    new LocalAdapter("local2", "D:\\Files")
};

var fileSystem = new FileSystem(adapters);
```

### Example operations

```
// Azure connection.
var azureClient = new ShareClient("connectionString", "shareName");

// Dropbox connection.
var dropboxClient = new DropboxClient("oAuth2AccessToken");

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new AzureFileStorageAdapter("azure", "/Files", azureClient),
    new DropboxAdapter("dropbox", "/Files", dropboxClient)
};

// Copies a file from the `local` adapter to the `azure` adapter.
await fileSystem.CopyFileAsync("local://foo/bar.txt", "azure://bar/foo.txt");

// Moves a file from the `azure` adapter to the `dropbox` adapter.
await fileSystem.MoveFileAsync("azure://Foo/Bar.txt", "dropbox://Bar/Foo.txt");

// Writes string contents to the `azure` adapter.
await fileSystem.WriteFileAsync("azure://Foo.txt", "Bar!");

// Reads a text file from the `dropbox` adapter.
var contents = fileSystem.ReadTextFileAsync("dropbox://Foo.txt");
```