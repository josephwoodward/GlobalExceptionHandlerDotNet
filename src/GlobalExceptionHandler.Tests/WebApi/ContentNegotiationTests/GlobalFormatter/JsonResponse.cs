using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

namespace GlobalExceptionHandler.Tests.WebApi.ContentNegotiationTests.GlobalFormatter
{
    public class JsonResponse : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private readonly HttpRequestMessage _requestMessage;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private const string ApiProductNotFound = "/api/productnotfound";
        private const string ErrorMessage = "Record could not be found";

        public JsonResponse(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.DefaultStatusCode = StatusCodes.Status404NotFound;
                    x.ResponseBody(ex => new TestResponse
                    {
                        Message = ex.Message
                    });
                });

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new RecordNotFoundException(ErrorMessage));
                });
            });

            _requestMessage = new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound);
            _requestMessage.Headers.Accept.Clear();
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _client = new TestServer(webHost).CreateClient();
        }

        [Fact]
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
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
            content.ShouldContain("{\"message\":\"" + ErrorMessage + "\"}");
        }

        public async Task InitializeAsync()
            => _response = await _client.SendAsync(_requestMessage);

        public Task DisposeAsync() 
            => Task.CompletedTask;
    }
}