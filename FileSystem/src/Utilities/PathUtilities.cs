using System;
using System.Linq;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;

namespace SharpGrip.FileSystem.Utilities
{
    public static class PathUtilities
    {
        private const string AdapterPrefixSeparator = "://";
        private const string PathSeparator = "/";
        private const string InvalidPathSeparator = "\\";

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
        /// Normalizes the adapter's root path.
        /// </summary>
        /// <param name="rootPath">The adapter's root path.</param>
        /// <returns>The path.</returns>
        public static string NormalizeRootPath(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath) || rootPath == PathSeparator)
            {
                return PathSeparator;
            }

            return rootPath.Replace(InvalidPathSeparator, PathSeparator).RemoveTrailingForwardSlash();
        }

        /// <summary>
        /// Normalizes a virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>The normalized virtual path.</returns>
        public static string NormalizeVirtualPath(string virtualPath)
        {
            if (!virtualPath.Contains(AdapterPrefixSeparator))
            {
                throw new InvalidVirtualPathException(virtualPath);
            }

            var prefixAndPath = ResolvePrefixAndPath(virtualPath);

            if (prefixAndPath.Length == 1)
            {
                return virtualPath;
            }

            return prefixAndPath[0] + AdapterPrefixSeparator + prefixAndPath[1].RemoveLeadingForwardSlash();
        }

        /// <summary>
        /// Returns the path from a prefixed path.
        /// </summary>
        /// <param name="virtualPath">The prefixed path.</param>
        /// <param name="rootPath">The adapter's root path.</param>
        /// <returns>The path.</returns>
        public static string GetPath(string virtualPath, string rootPath)
        {
            if (rootPath == PathSeparator)
            {
                rootPath = "";
            }

            var prefixAndPath = ResolvePrefixAndPath(virtualPath);

            if (prefixAndPath.Length == 1)
            {
                return string.Join(PathSeparator, rootPath);
            }

            return string.Join(PathSeparator, rootPath, prefixAndPath[1].RemoveLeadingForwardSlash());
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

            path = path.Replace(InvalidPathSeparator, PathSeparator);

            if (!rootPath.IsNullOrEmpty() && rootPath != "/")
            {
                path = path.ReplaceFirst(rootPath, "");
            }

            path = path.RemoveLeadingForwardSlash();

            return string.Join(AdapterPrefixSeparator, prefix, path);
        }

        /// <summary>
        /// Returns the parent path part from a path.
        /// </summary>
        /// <param name="path">The prefixed path.</param>
        /// <returns>The parent path part.</returns>
        public static string GetParentPathPart(string path)
        {
            if (path.IsNullOrEmpty())
            {
                path = "/";
            }

            var pathParts = GetPathParts(path);

            return string.Join("/", pathParts.Take(pathParts.Length - 1));
        }

        /// <summary>
        /// Returns the last path part from a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The last path part.</returns>
        public static string GetLastPathPart(string path)
        {
            if (path.IsNullOrEmpty() || path == PathSeparator)
            {
                return "";
            }

            return GetPathParts(path).Last();
        }

        /// <summary>
        /// Returns the path parts from a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The path parts.</returns>
        public static string[] GetPathParts(string path)
        {
            return path.Split(new[] {PathSeparator}, StringSplitOptions.RemoveEmptyEntries);
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

            return path.Split(new[] {AdapterPrefixSeparator}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}