using System.IO;

namespace SharpGrip.FileSystem.Adapters
{
    public abstract class Adapter
    {
        public string Prefix { get; }
        public string RootPath { get; }

        protected Adapter(string prefix, string rootPath)
        {
            Prefix = prefix;
            RootPath = rootPath;
        }

        protected string PrependRootPath(string path)
        {
            return Path.Combine(RootPath, path);
        }
    }
}