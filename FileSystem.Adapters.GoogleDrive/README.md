# SharpGrip FileSystem GoogleDrive adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.GoogleDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleDrive)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.GoogleDrive` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.GoogleDrive).

## Usage

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