using System;
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
    public class LogExceptionTests : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private Exception _exception;
        private string _contextType;
        private HandlerContext _handlerContext;
        private readonly TestServer _server;
        private int _statusCode;
        private const string RequestUri = "/api/productnotfound";

        public LogExceptionTests(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.OnException((ex, context) =>
                    {
                        _exception = ex;
                        _contextType = context.GetType().ToString();
                        return Task.CompletedTask;
                    });
                    x.Map<ArgumentException>().ToStatusCode(StatusCodes.Status404NotFound).WithBody(
                        (e, c, h) =>
                        {
                            _statusCode = c.Response.StatusCode;
                            _handlerContext = h;
                            return Task.CompletedTask;
                        });
                });

                app.Map(RequestUri, config =>
                {
                    config.Run(context => throw new ArgumentException("Invalid request"));
                });
            });

            _server = new TestServer(webHost);
        }

        public async Task InitializeAsync()
        {
            using (var client = _server.CreateClient())
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), RequestUri);
                await client.SendAsync(requestMessage);
            }
        }

        [Fact]
        public void Invoke_logger()
        {
            _exception.ShouldBeOfType<ArgumentException>();
        }

        [Fact]
        public void HttpContext_is_set()
        {
            _contextType.ShouldBe("Microsoft.AspNetCore.Http.DefaultHttpContext");
        }

        [Fact]
        public void Handler_context_is_set()
        {
            _handlerContext.ShouldBeOfType<HandlerContext>();
        }

        [Fact]
        public void Status_code_is_set()
        {
            _statusCode.ShouldBe(StatusCodes.Status404NotFound);
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}