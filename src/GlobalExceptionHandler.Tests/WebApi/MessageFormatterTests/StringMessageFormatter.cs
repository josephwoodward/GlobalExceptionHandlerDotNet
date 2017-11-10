using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using GlobalExceptionHandler.Tests.WebApi.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.MessageFormatterTests
{
    public class StringMessageFormatter : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;
        private const string Response = "Hello World!";
        
        public StringMessageFormatter(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
                app.UseExceptionHandler().WithConventions(x =>
                {
                    x.ContentType = "application/json";
                    x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound)
                        .UsingMessageFormatter((exception, context) => Response);
                });

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new RecordNotFoundException("Record could not be found"));
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
        public async Task Correct_response_message()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBe(Response);
        }
    }
}