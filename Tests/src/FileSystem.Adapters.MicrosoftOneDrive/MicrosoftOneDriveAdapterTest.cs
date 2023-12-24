using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.MicrosoftOneDrive;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.MicrosoftOneDrive
{
    public class MicrosoftOneDriveAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
            var delegateAuthenticationProvider = new DelegateAuthenticationProvider(message => Task.FromResult(message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "12345")));
            var graphServiceClient = Substitute.For<GraphServiceClient>(delegateAuthenticationProvider, null);
            var microsoftOneDriveAdapter = new MicrosoftOneDriveAdapter("prefix", "/root-path", graphServiceClient, "driveId");

            Assert.Equal("prefix", microsoftOneDriveAdapter.Prefix);
            Assert.Equal("/root-path", microsoftOneDriveAdapter.RootPath);
        }

        [Fact]
        public Task Test_Connect()
        {
            var delegateAuthenticationProvider = new DelegateAuthenticationProvider(message => Task.FromResult(message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "12345")));
            var graphServiceClient = Substitute.For<GraphServiceClient>(delegateAuthenticationProvider, null);
            var microsoftOneDriveAdapter = new MicrosoftOneDriveAdapter("prefix", "/root-path", graphServiceClient, "driveId");

            microsoftOneDriveAdapter.Connect();

            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_Directory_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_Files_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Get_Directories_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_File_Exists_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Directory_Exists_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Create_Directory_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Delete_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Delete_Directory_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Read_File_Stream_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Read_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Read_Text_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Write_File_Async()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public Task Test_Append_File_Async()
        {
            return Task.CompletedTask;
        }
    }
}