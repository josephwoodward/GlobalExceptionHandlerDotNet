using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.GlobalFormatterTests
{
    public class FallBackResponseTest : IClassFixture<WebApiServerFixture>
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private readonly HttpClient _client;

        public FallBackResponseTest(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x => {
                    x.ContentType = "application/json";
                    x.ResponseBody(s => JsonConvert.SerializeObject(new
                    {
                        Message = "An error occured whilst processing your request"
                    }));
                    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound);
                });

                app.Map(ApiProductNotFound, config => { config.Run(context => throw new RecordNotFoundException()); });
            });

            _client = new TestServer(webHost).CreateClient();
        }

        [Fact]
        public async Task Returns_correct_response_type()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public async Task Returns_correct_status_code()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_global_exception_message()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldBe("{\"Message\":\"An error occured whilst processing your request\"}");
        }
    }
}