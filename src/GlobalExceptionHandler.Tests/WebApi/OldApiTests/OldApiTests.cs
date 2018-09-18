using System;
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

namespace GlobalExceptionHandler.Tests.WebApi.OldApiTests
{
    public class OldApiTests : IClassFixture<WebApiServerFixture>
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private HttpClient _client;

        public OldApiTests(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound);
                    x.ResponseBody(exception => JsonConvert.SerializeObject(new
                    {
                        error = new
                        {
                            exception = exception.GetType().Name,
                            message = exception.Message
                        }
                    }));
                });

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new NullReferenceException("Object is null"));
                });
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
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldBe("{\"error\":{\"exception\":\"NullReferenceException\",\"message\":\"Object is null\"}}");
        }
    }
}