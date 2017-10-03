using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.WebApi.Fixtures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi
{
    public class FailingTests : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;

        public FailingTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
                app.UseWebApiGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.MessageFormatter(exception => JsonConvert.SerializeObject(new
                    {
                        error = new
                        {
                            message = "Something went wrong!"
                        }
                    }));
                    x.ForException<DivideByZeroException>().ReturnStatusCode(HttpStatusCode.BadRequest).UsingMessageFormatter(
                        exception => JsonConvert.SerializeObject(new
                        {
                            error = new
                            {
                                exception = exception.GetType().Name,
                                message = exception.Message
                            }
                        }));
                });

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new ArgumentException("Can't divide by zero"));
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
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public void Returns_correct_status_code()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldContain("Something went wrong!");
        }
    }
}