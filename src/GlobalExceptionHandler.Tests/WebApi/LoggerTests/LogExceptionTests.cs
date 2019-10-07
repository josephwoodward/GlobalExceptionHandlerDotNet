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
        private HttpContext _context;
        private HandlerContext _handlerContext;
        private readonly TestServer _server;
        private const string RequestUri = "/api/productnotfound";

        public LogExceptionTests(WebApiServerFixture fixture)
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
                await Task.Delay(1000);
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
            _context.ShouldBeOfType<DefaultHttpContext>();
        }
        
        [Fact]
        public void Handler_context_is_set()
        {
            _handlerContext.ShouldBeOfType<HandlerContext>();
        }
        
        [Fact]
        public void Status_code_is_set()
        {
            _context.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}