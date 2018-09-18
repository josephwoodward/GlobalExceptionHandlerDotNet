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

namespace GlobalExceptionHandler.Tests.WebApi.StatusCodeTests
{
    public class BasicTestsWithRunTimeStatusCode : IClassFixture<WebApiServerFixture>
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private readonly HttpClient _client;

        public BasicTestsWithRunTimeStatusCode(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.Map<ArgumentException>().ToStatusCode(ex => ex is ArgumentException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError);
                    x.ResponseBody(c => JsonConvert.SerializeObject(new TestResponse
                    {
                        Message = c.Message
                    }));
                });

                app.Map(requestUri, config => { config.Run(context => throw new ArgumentException("Invalid request")); });
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
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }
}