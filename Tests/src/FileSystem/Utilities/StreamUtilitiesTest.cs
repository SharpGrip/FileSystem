using System.IO;
using System.Threading.Tasks;
using SharpGrip.FileSystem.Utilities;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Utilities
{
    public class StreamUtilitiesTest
    {
        [Fact]
        public async Task Test_CopyContentsToMemoryStreamAsync()
        {
            var memoryStream1 = new MemoryStream("test"u8.ToArray());
            var memoryStream2 = await StreamUtilities.CopyContentsToMemoryStreamAsync(memoryStream1);

            var memoryStreamContents1 = await new StreamReader(memoryStream1).ReadToEndAsync();
            var memoryStreamContents2 = await new StreamReader(memoryStream2).ReadToEndAsync();

            Assert.Equal(memoryStreamContents1, memoryStreamContents2);
        }
    }
}