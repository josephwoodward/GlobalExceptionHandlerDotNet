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
    public class BareMetalTests : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
        private const string ApiProductNotFound = "/api/productnotfound";
        private readonly HttpClient _client;
        private HttpResponseMessage _response;

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
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.ShouldBeNull();
        }

        [Fact]
        public void Returns_correct_status_code()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Returns_empty_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBeEmpty();
        }

        public async Task InitializeAsync()
        {
            _response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
        }

        public Task DisposeAsync()
            => Task.CompletedTask;
    }
}