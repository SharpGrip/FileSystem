# SharpGrip FileSystem

## Introduction
SharpGrip FileSystem is a file system abstraction supporting multiple adapters.

## Usage
```
var adapters = new List<IAdapter>
{
    new LocalAdapter("uploads", Path.Combine("Path", "to", "uploads", "file", "system")),
    new LocalAdapter("archive", Path.Combine("Path", "to", "archive", "file", "system")),
};

var fileSystem = new FileSystem(adapters);

await fileSystem.CopyFile("uploads://foo.txt", "archive://bar.txt", true);
```

## Supported adapters
- Local
