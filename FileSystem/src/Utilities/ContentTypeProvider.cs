using MimeTypes;

namespace SharpGrip.FileSystem.Utilities
{
    public static class ContentTypeProvider
    {
        public static string GetContentType(string path)
        {
            return MimeTypeMap.GetMimeType(path);
        }
    }
}