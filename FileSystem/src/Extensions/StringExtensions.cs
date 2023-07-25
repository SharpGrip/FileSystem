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
            if (value.StartsWith("/"))
            {
                value = value.Substring(1);
            }

            return value;
        }

        public static string RemoveTrailingForwardSlash(this string value)
        {
            if (value.EndsWith("/"))
            {
                value = value.Remove(value.Length - 1);
            }

            return value;
        }

        public static string EnsureTrailingForwardSlash(this string value)
        {
            if (!value.EndsWith("/"))
            {
                value += "/";
            }

            return value;
        }
    }
}