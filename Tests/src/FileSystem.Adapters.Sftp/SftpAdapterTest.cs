using System.Threading.Tasks;
using Moq;
using Renci.SshNet;
using SharpGrip.FileSystem.Adapters.Sftp;
using SharpGrip.FileSystem.Exceptions;
using Xunit;

namespace Tests.FileSystem.Adapters.Sftp
{
    public class SftpAdapterTest
    {
        [Fact]
        public void Test_Instantiation()
        {
            var sftpClient = new Mock<SftpClient>("hostName", "userName", "password");
            var sftpAdapter = new SftpAdapter("prefix", "/root-path", sftpClient.Object);

            Assert.Equal("prefix", sftpAdapter.Prefix);
            Assert.Equal("/root-path", sftpAdapter.RootPath);
        }

        [Fact]
        public void Test_Connect()
        {
            var sftpClient = new Mock<SftpClient>("hostName", "userName", "password");
            var sftpAdapter = new SftpAdapter("prefix-1", "/root-path-1", sftpClient.Object);

            Assert.Throws<ConnectionException>(() => sftpAdapter.Connect());
        }

        [Fact]
        public async Task Test_Get_File_Async()
        {
            var sftpClient = new Mock<SftpClient>("hostName", "userName", "password");
            var sftpAdapter = new SftpAdapter("prefix-1", "/root-path-1", sftpClient.Object);

            await Assert.ThrowsAsync<ConnectionException>(async () => await sftpAdapter.GetFileAsync("prefix-1://test.txt"));
        }
    }
}