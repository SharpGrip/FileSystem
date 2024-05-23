# SharpGrip FileSystem MicrosoftOneDrive adapter [![NuGet](https://img.shields.io/nuget/v/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive)](https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive)

## Installation

Reference NuGet package `SharpGrip.FileSystem.Adapters.MicrosoftOneDrive` (https://www.nuget.org/packages/SharpGrip.FileSystem.Adapters.MicrosoftOneDrive).

## Usage

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