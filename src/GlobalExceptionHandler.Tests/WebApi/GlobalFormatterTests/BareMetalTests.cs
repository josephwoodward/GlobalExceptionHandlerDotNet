using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.GlobalFormatterTests
{
    public class BareMetalTests : IClassFixture<WebApiServerFixture>
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private readonly HttpClient _client;

        public BareMetalTests(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
                app.UseGlobalExceptionHandler();

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new ArgumentException("Invalid request"));
                });
                app.Map("/test", config =>
                {
                    config.Run(context => Task.FromResult("Working"));
                });
            });

            _client = new TestServer(webHost).CreateClient();
        }

        [Fact]
        public async Task Returns_correct_response_type()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            response.Content.Headers.ContentType.ShouldBeNull();
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
            content.ShouldBeEmpty();
        }
    }
}