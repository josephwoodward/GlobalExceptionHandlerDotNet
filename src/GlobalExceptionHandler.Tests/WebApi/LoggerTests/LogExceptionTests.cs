using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.WebApi.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.LoggerTests
{
    public class LogExceptionTests : IClassFixture<WebApiServerFixture>
    {
        private Exception _exception;
        private HttpContext _context;

        public LogExceptionTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
                app.UseWebApiGlobalExceptionHandler(x =>
                {
                    x.OnError((ex, context) =>
                    {
                        _exception = ex;
                        _context = context;
                        return Task.CompletedTask;
                    });
                    x.ForException<ArgumentException>().ReturnStatusCode(HttpStatusCode.InternalServerError);
                });

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new ArgumentException("Invalid request"));
                });
            });

            // Act
            var server = new TestServer(webHost);
            using (var client = server.CreateClient())
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), requestUri);
                client.SendAsync(requestMessage).Wait();
                Task.Delay(1000);
            }
        }
        
        [Fact]
        public void Invoke_logger()
        {
            _exception.ShouldBeOfType<ArgumentException>();
        }
        
        [Fact]
        public void Context_is_set()
        {
            _context.ShouldBeOfType<DefaultHttpContext>();
        }
    }
}