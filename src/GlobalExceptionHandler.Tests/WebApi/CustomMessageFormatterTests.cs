using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi
{
    public class CustomMessageFormatterTests : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;
        private readonly string ExceptionMessage = "Product could not be found";
        public CustomMessageFormatterTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string RequestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app => {

                app.UseWebApiGlobalExceptionHandler(x => {
                    x.ContentType = "application/json";
                    x.ForException<ProductNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
                    x.MessageFormatter((e) => e.Message);
                });

                app.Map(RequestUri, config => {
                    config.Run(context => {
                        throw new ProductNotFoundException(ExceptionMessage);
                    });
                });

            });

            // Act
            var server = new TestServer(webHost);
            using(var client = server.CreateClient()){
                var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), RequestUri);
                _response = client.SendAsync(requestMessage).Result;
            }
        }

        [Fact]
        public void Should_return_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public void Should_return_correct_status_code()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_return_custom_message()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBe(ExceptionMessage);
        }
    }
}