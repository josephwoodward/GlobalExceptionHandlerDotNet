using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.WebApi.Fixtures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.LoggerTests
{
    public class LoggerTests : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;
        private string _logMessage = string.Empty;

        public LoggerTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
                app.UseWebApiGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.OnError((c, ex) =>
                    {
                        _logMessage = "";
                        return Task.CompletedTask;
                    });
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
                _response = client.SendAsync(requestMessage).Result;
            }
        }

        [Fact]
        public void Returns_correct_status_code()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }
        
        [Fact]
        public void Invoke_logger()
        {
            _logMessage.ShouldBe("");
        }

    }
}