using System.IO;

namespace SharpGrip.FileSystem.Adapters
{
    public abstract class Adapter
    {
        private readonly string _rootPath;

        public string Prefix { get; }

        protected Adapter(string prefix, string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                throw new DirectoryNotFoundException($"Directory '{rootPath}' not found.");
            }

            Prefix = prefix;
            _rootPath = rootPath;
        }

        protected string PrependRootPath(string path)
        {
            return Path.Combine(_rootPath, path);
        }
    }
}