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

namespace GlobalExceptionHandler.Tests.WebApi.GlobalFormatterTests
{
    public class BasicWithOverrideTests : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;

        public BasicWithOverrideTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {
                app.UseExceptionHandler().WithConventions(x =>
                {
                    x.ContentType = "application/json";
                    x.ForException<NeverThrownException>().ReturnStatusCode(StatusCodes.Status400BadRequest);
                    x.MessageFormatter(exception => JsonConvert.SerializeObject(new
                    {
                        error = new
                        {
                            message = "Something went wrong"
                        }
                    }));
                });

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new ArgumentException("Invalid request"));
                });
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
            _response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBe(@"{""error"":{""message"":""Something went wrong""}}");
        }
    }
}