using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Http;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.GoogleDrive;

public class TestHttpExecuteInterceptor : IHttpExecuteInterceptor
{
    public Task InterceptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine(request.RequestUri);

        return Task.CompletedTask;
    }
}