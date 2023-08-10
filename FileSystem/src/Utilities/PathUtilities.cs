using System;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;

namespace SharpGrip.FileSystem.Utilities
{
    public static class PathUtilities
    {
        private const string AdapterPrefixSeparator = "://";
        private const string PathSeparator = "/";

        /// <summary>
        /// Returns the prefix from a prefixed path.
        /// </summary>
        /// <param name="virtualPath">The prefixed path.</param>
        /// <returns>The prefix.</returns>
        public static string GetPrefix(string virtualPath)
        {
            return ResolvePrefixAndPath(virtualPath)[0];
        }

        /// <summary>
        /// Returns the path from a prefixed path.
        /// </summary>
        /// <param name="virtualPath">The prefixed path.</param>
        /// <param name="rootPath">The adapter's root path.</param>
        /// <returns>The path.</returns>
        public static string GetPath(string virtualPath, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                return string.Join(PathSeparator, ResolvePrefixAndPath(virtualPath)[1]);
            }

            return string.Join(PathSeparator, rootPath, ResolvePrefixAndPath(virtualPath)[1]);
        }

        /// <summary>
        /// Returns the virtual path from a real path.
        /// </summary>
        /// <param name="path">The prefixed path.</param>
        /// <param name="prefix">The adapter's prefix.</param>
        /// <param name="rootPath">The adapter root path.</param>
        /// <returns>The virtual path.</returns>
        public static string GetVirtualPath(string path, string prefix, string rootPath)
        {
            if (path.Contains(AdapterPrefixSeparator))
            {
                throw new InvalidPathException(path);
            }

            path = path.Replace('\\', '/');

            if (!rootPath.IsNullOrEmpty() && rootPath != "/")
            {
                path = path.Replace(rootPath, "");
            }

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            return string.Join(AdapterPrefixSeparator, prefix, path);
        }

        /// <summary>
        /// Resolves the prefix and path from a prefixed path.
        /// </summary>
        /// <param name="path">The prefixed path.</param>
        /// <returns>The prefix and path.</returns>
        private static string[] ResolvePrefixAndPath(string path)
        {
            if (!path.Contains(AdapterPrefixSeparator))
            {
                throw new PrefixNotFoundInPathException(path);
            }

            return path.Split(new[] {AdapterPrefixSeparator}, StringSplitOptions.None);
        }
    }
}