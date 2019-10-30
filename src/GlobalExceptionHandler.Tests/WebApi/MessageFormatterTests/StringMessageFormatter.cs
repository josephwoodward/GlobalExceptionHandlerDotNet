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
    public class StringMessageFormatter : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private const string Response = "Hello World!";
        private const string ApiProductNotFound = "/api/productnotfound";

        public StringMessageFormatter(WebApiServerFixture fixture)
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
                    config.Run(context => throw new RecordNotFoundException("Record could not be found"));
                });
            });

            _client = new TestServer(webHost).CreateClient();
        }

        public async Task InitializeAsync()
        {
            _response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
        }

        [Fact]
        public async Task Correct_response_message()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBe(Response);
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}