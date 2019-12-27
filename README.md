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

var filesystem = new FileSystem(adapters);

await filesystem.CopyFile("uploads://foo.txt", "archive://bar.txt", true);
```

## Supported adapters
- Local
