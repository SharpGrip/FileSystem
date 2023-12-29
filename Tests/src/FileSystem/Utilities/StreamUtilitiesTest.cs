using System.IO;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Utilities;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Utilities
{
    public class StreamUtilitiesTest
    {
        [Fact]
        public async Task Test_CopyContentsToMemoryStreamAsync_PositionToStart_True()
        {
            var memoryStream1 = new MemoryStream("test"u8.ToArray());
            var memoryStream2 = await StreamUtilities.CopyContentsToMemoryStreamAsync(memoryStream1, true);

            memoryStream1.Seek(0, SeekOrigin.Begin);

            var memoryStreamContents1 = await new StreamReader(memoryStream1).ReadToEndAsync();
            var memoryStreamContents2 = await new StreamReader(memoryStream2).ReadToEndAsync();

            Assert.Equal(memoryStreamContents1, memoryStreamContents2);
        }

        [Fact]
        public async Task Test_CopyContentsToMemoryStreamAsync_PositionToStart_False()
        {
            var memoryStream1 = new MemoryStream("test"u8.ToArray());
            var memoryStream2 = await StreamUtilities.CopyContentsToMemoryStreamAsync(memoryStream1, false);

            var memoryStreamContents1 = await new StreamReader(memoryStream1).ReadToEndAsync();
            var memoryStreamContents2 = await new StreamReader(memoryStream2).ReadToEndAsync();

            Assert.Equal(memoryStreamContents1, memoryStreamContents2);
        }
    }
}