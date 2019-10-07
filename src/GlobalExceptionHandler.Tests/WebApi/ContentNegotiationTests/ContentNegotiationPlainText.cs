using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GlobalExceptionHandler.ContentNegotiation.Mvc;
using GlobalExceptionHandler.Tests.Exceptions;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.ContentNegotiationTests
{
    public class ContentNegotiationPlainText : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private readonly HttpRequestMessage _requestMessage;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;

        public ContentNegotiationPlainText(WebApiServerFixture fixture)
        {
            // Arrange

            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
                        .WithBody((e, c, h) => c.WriteAsyncObject(e.Message));
                });

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new RecordNotFoundException("Record could not be found"));
                });
            });

            _requestMessage = new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound);
            _requestMessage.Headers.Accept.Clear();
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            _client = new TestServer(webHost).CreateClient();
        }
        
        [Fact]
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
        }

        [Fact]
        public void Returns_correct_status_code()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldContain("Record could not be found");
        }

        public async Task InitializeAsync()
        {
            _response = await _client.SendAsync(_requestMessage);
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}