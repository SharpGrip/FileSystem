using SharpGrip.FileSystem.Utilities;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Utilities
{
    public class ContentTypeProviderTest
    {
        [Fact]
        public void Test_GetContentType()
        {
            Assert.Equal("text/plain", ContentTypeProvider.GetContentType("text.txt"));
            Assert.Equal("application/pdf", ContentTypeProvider.GetContentType("text.pdf"));
            Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ContentTypeProvider.GetContentType("text.docx"));
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ContentTypeProvider.GetContentType("text.xlsx"));
            Assert.Equal("application/vnd.openxmlformats-officedocument.presentationml.presentation", ContentTypeProvider.GetContentType("text.pptx"));
        }
    }
}