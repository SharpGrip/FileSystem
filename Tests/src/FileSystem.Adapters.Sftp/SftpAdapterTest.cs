using NSubstitute;
using Renci.SshNet;
using SharpGrip.FileSystem.Adapters.Sftp;
using SharpGrip.FileSystem.Exceptions;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.Sftp
{
    public class SftpAdapterTest
    {
        [Fact]
        public void Test_Instantiation()
        {
            var sftpClient = Substitute.For<SftpClient>("hostName", "userName", "password");
            var sftpAdapter = new SftpAdapter("prefix", "/root-path", sftpClient);

            Assert.Equal("prefix", sftpAdapter.Prefix);
            Assert.Equal("/root-path", sftpAdapter.RootPath);
        }

        [Fact]
        public void Test_Connect()
        {
            var sftpClient = Substitute.For<SftpClient>("hostName", "userName", "password");
            var sftpAdapter = new SftpAdapter("prefix-1", "/root-path-1", sftpClient);

            Assert.Throws<ConnectionException>(() => sftpAdapter.Connect());
        }
    }
}