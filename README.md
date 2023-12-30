# SharpGrip FileSystem [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem)](https://www.nuget.org/packages/SharpGrip.FileSystem)

## Builds

[![FileSystem [Build]](https://github.com/SharpGrip/FileSystem/actions/workflows/Build.yaml/badge.svg)](https://github.com/SharpGrip/FileSystem/actions/workflows/Build.yaml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=alert_status)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=sqale_rating)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=reliability_rating)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=security_rating)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem) \
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SharpGrip_FileSystem&metric=coverage)](https://sonarcloud.io/summary/overall?id=SharpGrip_FileSystem)

## Introduction

SharpGrip FileSystem is a .NET file system abstraction supporting multiple adapters.

## Installation

Reference NuGet package `SharpGrip.FileSystem` (https://www.nuget.org/packages/SharpGrip.FileSystem).

For adapters other than the local file system (included in the `SharpGrip.FileSystem` package) please see the [Supported adapters](#supported-adapters) section.

## Supported adapters

| Adapter                                         | Package                                           | NuGet                                                                                                                                                                      |
|:------------------------------------------------|:--------------------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Local adapter](#local-adapter)                 | `SharpGrip.FileSystem`                            | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem)](https://www.nuget.org/packages/SharpGrip.FileSystem)                                                       |
| [AmazonS3](#amazons3-adapter)                   | `SharpGrip.FileSystem.Adapters.AmazonS3`          | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AmazonS3)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AmazonS3)                   |
| [AzureBlobStorage](#azureblobstorage-adapter)   | `SharpGrip.FileSystem.Adapters.AzureBlobStorage`  | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AzureBlobStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureBlobStorage)   |
| [AzureFileStorage](#azurefilestorage-adapter)   | `SharpGrip.FileSystem.Adapters.AzureFileStorage`  | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.AzureFileStorage)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.AzureFileStorage)   |
| [Dropbox](#dropbox-adapter)                     | `SharpGrip.FileSystem.Adapters.Dropbox`           | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Dropbox)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Dropbox)                     |
| [GoogleDrive](#googledrive-adapter)             | `SharpGrip.FileSystem.Adapters.GoogleDrive`       | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.GoogleDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleDrive)             |
| [MicrosoftOneDrive](#microsoftonedrive-adapter) | `SharpGrip.FileSystem.Adapters.MicrosoftOneDrive` | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive) |
| [SFTP](#sftp-adapter)                           | `SharpGrip.FileSystem.Adapters.Sftp`              | [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.Sftp)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.Sftp)                           |

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

### AmazonS3 adapter

```
// Amazon connection.
var amazonClient = new AmazonS3Client("awsAccessKeyId", "awsSecretAccessKey", RegionEndpoint.USEast2);

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new AmazonS3Adapter("amazon", "/Files", amazonClient, "bucketName")
};

var fileSystem = new FileSystem(adapters);
```

### AzureBlobStorage adapter

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

### AzureFileStorage adapter

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

### Dropbox adapter

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

### GoogleDrive adapter

```
// Google connection.
await using var stream = new FileStream("path/to/credentials.json", FileMode.Open, FileAccess.Read);
const string tokenPath = "path/to/token/directory";
var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
    new[] {DriveService.Scope.Drive},
    "user",
    CancellationToken.None,
    new FileDataStore(tokenPath, true));

var googleDriveClient = new DriveService(new BaseClientService.Initializer
{
    HttpClientInitializer = credential,
    ApplicationName = "Test"
});

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new GoogleDriveAdapter("google-drive", "/Files", googleDriveClient)
};

var fileSystem = new FileSystem(adapters);
```

### MicrosoftOneDrive adapter

```
// Microsoft connection.
var scopes = new[] {"https://graph.microsoft.com/.default"};
var tenantId = "tenantId";
var confidentialClient = ConfidentialClientApplicationBuilder
    .Create("clientId")
    .WithAuthority($"https://login.microsoftonline.com/{tenantId}/v2.0")
    .WithClientSecret("clientSecret")
    .Build();
var oneDriveClient = new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage =>
    {
        var authResult = await confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
    })
);

var adapters = new List<IAdapter>
{
    new LocalAdapter("local", "/var/files"),
    new MicrosoftOneDriveAdapter("onedrive", "/Files", oneDriveClient, "driveId")
};

var fileSystem = new FileSystem(adapters);
```

### SFTP adapter

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