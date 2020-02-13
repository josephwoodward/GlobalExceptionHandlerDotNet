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
    public class UnhandledExceptionLoggerTests : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private readonly TestServer _server;
        private Type _matchedException;
        private Exception _exception;
        private const string RequestUri = "/api/productnotfound";

        public UnhandledExceptionLoggerTests(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.OnException((context, _) =>
                    {
                        _matchedException = context.ExceptionMatched;
                        _exception = context.Exception;

                        return Task.CompletedTask;
                    });
                    x.Map<ArgumentException>().ToStatusCode(StatusCodes.Status404NotFound).WithBody((e, c, h) => Task.CompletedTask);
                });

                app.Map(RequestUri, config =>
                {
                    config.Run(context => throw new NotImplementedException("Method not implemented"));
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
        public void ExceptionMatchIsNotSet()
            => _matchedException.ShouldBeNull();

        [Fact]
        public void ExceptionTypeIsCorrect()
            => _exception.ShouldBeOfType<NotImplementedException>();

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}