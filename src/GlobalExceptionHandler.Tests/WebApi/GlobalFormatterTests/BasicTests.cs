using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static System.Threading.Tasks.Task;

namespace GlobalExceptionHandler.Tests.WebApi.GlobalFormatterTests
{
    public class BasicTests : IClassFixture<WebApiServerFixture>
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private readonly HttpClient _client;

        public BasicTests(WebApiServerFixture fixture, ITestOutputHelper output)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.ResponseBody(c => JsonConvert.SerializeObject(new TestResponse
                    {
                        Message = c.Message
                    }));
                }, LogFactory.Create(output));

                app.Map(ApiProductNotFound, config => { config.Run(context => throw new ArgumentException("Invalid request")); });
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
        public async Task Returns_empty_body()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldBe("{\"Message\":\"Invalid request\"}");
        }
    }
}