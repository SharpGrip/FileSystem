using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Constants;

namespace SharpGrip.FileSystem.Utilities
{
    public static class StreamUtilities
    {
        public static async Task<MemoryStream> CopyContentsToMemoryStreamAsync(Stream sourceStream, bool setPositionToStart, CancellationToken cancellationToken = default)
        {
            var memoryStream = new MemoryStream();
            await sourceStream.CopyToAsync(memoryStream, FileSystemConstants.Streaming.DefaultMemoryStreamBufferSize, cancellationToken);

            if (setPositionToStart)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
            }

            return memoryStream;
        }
    }
}