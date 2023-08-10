using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Constants;

namespace SharpGrip.FileSystem.Utilities
{
    public static class StreamUtilities
    {
        public static async Task<MemoryStream> CopyContentsToMemoryStreamAsync(Stream contents, CancellationToken cancellationToken = default)
        {
            var memoryStream = new MemoryStream();
            contents.Seek(0, SeekOrigin.Begin);
            await contents.CopyToAsync(memoryStream, AdapterConstants.DefaultMemoryStreamBufferSize, cancellationToken);

            return memoryStream;
        }
    }
}