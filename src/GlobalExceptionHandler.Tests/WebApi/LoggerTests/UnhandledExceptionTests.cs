using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.LoggerTests
{
    public class UnhandledExceptionTests : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private Exception _exception;
        private HttpContext _context;
        private HandlerContext _handlerContext;
        private readonly TestServer _server;
        private const string RequestUri = "/api/productnotfound";

        public UnhandledExceptionTests(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.OnError((ex, context) =>
                    {
                        _exception = ex;
                        _context = context;
                        return Task.CompletedTask;
                    });
                    x.Map<ArgumentException>().ToStatusCode(StatusCodes.Status404NotFound).WithBody(
                        (e, c, h) =>
                        {
                            _exception = e;
                            _context = c;
                            _handlerContext = h;

                            return Task.CompletedTask;
                        });
                });

                app.Map(RequestUri, config =>
                {
                    config.Run(context => throw new HttpRequestException("Something went wrong"));
                });
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
            if (_exception.Message.Contains("unhandled exception"))
            {
                "true".ShouldBe("Contains unhandled exception");
            } else
            {
                "true".ShouldBe("Does not contain unhandled exception");
            }
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}