using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using NSubstitute;
using SharpGrip.FileSystem.Adapters.MicrosoftOneDrive;
using Xunit;

namespace Tests.FileSystem.Adapters.MicrosoftOneDrive
{
    public class MicrosoftOneDriveAdapterTest
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
    }
}