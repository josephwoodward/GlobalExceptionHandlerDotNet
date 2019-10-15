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
    public class XmlResponse : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private const string ContentType = "application/xml";
        private const string ApiProductNotFound = "/api/productnotfound";
        private const string ErrorMessage = "Record could not be found";

        public XmlResponse(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithXmlFormatters();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
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


            _client = new TestServer(webHost).CreateClient();
        }

        [Fact]
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe(ContentType);
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
            content.ShouldContain($"<Message>{ErrorMessage}</Message>");
        }

        public async Task InitializeAsync()
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound);
            requestMessage.Headers.Accept.Clear();
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            
            _response = await _client.SendAsync(requestMessage);
        }

        public Task DisposeAsync() 
            => Task.CompletedTask;
    }
}