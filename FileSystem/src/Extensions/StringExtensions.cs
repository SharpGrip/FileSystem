using System;

namespace SharpGrip.FileSystem.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string RemoveLeadingForwardSlash(this string value)
        {
            return value.TrimStart('/');
        }

        public static string RemoveTrailingForwardSlash(this string value)
        {
            return value.TrimEnd('/');
        }

        public static string EnsureTrailingForwardSlash(this string value)
        {
            if (!value.EndsWith("/"))
            {
                value += "/";
            }

            return value;
        }

        public static string EnsureLeadingForwardSlash(this string value)
        {
            if (!value.StartsWith("/"))
            {
                value = "/" + value;
            }

            return value;
        }

        public static string ReplaceFirst(this string value, string search, string replace)
        {
            var index = value.IndexOf(search, StringComparison.Ordinal);

            if (index < 0)
            {
                return value;
            }

            return value.Substring(0, index) + replace + value.Substring(index + search.Length);
        }
    }
}