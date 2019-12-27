using System.Collections.Generic;
using System.IO;

namespace SharpGrip.FileSystem
{
    public interface IFileSystem
    {
        FileInfo GetFile(string path);
        DirectoryInfo GetDirectory(string path);
        IEnumerable<FileInfo> GetFiles(string path = "");
        IEnumerable<DirectoryInfo> GetDirectories(string path = "");
    }
}