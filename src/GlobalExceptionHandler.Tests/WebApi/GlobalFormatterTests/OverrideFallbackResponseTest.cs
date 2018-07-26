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
    public class OverrideFallbackResponseTest : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;

        public OverrideFallbackResponseTest(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x => {
                    x.ContentType = "application/json";
                    x.DefaultResponseBody(s => JsonConvert.SerializeObject(new
                    {
                        Message = "An error occured whilst processing your request"
                    }));
                    
                    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound).WithBody((e, c) => JsonConvert.SerializeObject(new {e.Message}));
                });

                app.Map(requestUri, config => { config.Run(context => throw new RecordNotFoundException("Record could not be found")); });
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
            _response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_global_exception_message()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBe("{\"Message\":\"Record could not be found\"}");
        }
    }
}