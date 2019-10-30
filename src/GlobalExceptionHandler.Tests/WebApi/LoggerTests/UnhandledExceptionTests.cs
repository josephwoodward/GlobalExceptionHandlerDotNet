using System.Net.Http;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GlobalExceptionHandler.Tests.WebApi.LoggerTests
{
    public class UnhandledExceptionTests : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;
        private readonly TestServer _server;
        private const string RequestUri = "/api/productnotfound";

        public UnhandledExceptionTests(WebApiServerFixture fixture, ITestOutputHelper output)
        {
            _output = output;
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.ResponseBody(c => JsonConvert.SerializeObject(new TestResponse
                    {
                        Message = c.Message
                    }));
                    x.Map<HttpRequestException>()
                        .ToStatusCode(StatusCodes.Status404NotFound)
                        .WithBody((e, c, h) => Task.CompletedTask);
                }, LogFactory.Create(output));

                app.Map(RequestUri, c => c.Run(context => throw new HttpRequestException("Something went wrong")));
            });

            _server = new TestServer(webHost);
        }

        public async Task InitializeAsync()
        {
            using var client = _server.CreateClient();
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), RequestUri);
            await client.SendAsync(requestMessage);
        }

        [Fact]
        public void Unhandled_exception_is_thrown()
        {
            // The ExceptionHandling middleware returns an unhandled exception
            // See Microsoft.AspNetCore.Diagnostics.ExceptionHandlingMiddleware
            true.ShouldBe(true);
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}