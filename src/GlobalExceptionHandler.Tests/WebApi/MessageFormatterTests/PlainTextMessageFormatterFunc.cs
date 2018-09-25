using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.MessageFormatterTests
{
    public class PlainTextMessageFormatterFunc : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpClient _client;
        private const string Response = "Record could not be found";
        private const string ApiProductNotFound = "/api/productnotfound";

        public PlainTextMessageFormatterFunc(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
                        .WithBody((exception, context) => Response);
                });

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new RecordNotFoundException(Response));
                });
            });

            _client = new TestServer(webHost).CreateClient();
        }

        [Fact]
        public async Task Correct_response_message()
        {
            // Act
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            content.ShouldBe(Response);
        }
    }
}