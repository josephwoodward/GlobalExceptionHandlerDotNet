using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi
{
    public class CustomMessageFormatterTests : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;
        private const string ExceptionMessage = "Product could not be found";

        public CustomMessageFormatterTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHost();

            webHost.Configure(app =>
            {
                app.UseWebApiGlobalExceptionHandler(x =>
                {
                    x.ContentType = "application/json";
                    x.ForException<ProductNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
                    x.MessageFormatter(exception => JsonConvert.SerializeObject(new
                    {
                        error = new
                        {
                            exception = exception.GetType().Name,
                            message = exception.Message
                        }
                    }));
                });

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new ProductNotFoundException(ExceptionMessage));
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
            _response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_custom_message()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldBe(@"{""error"":{""exception"":""ProductNotFoundException"",""message"":""Product could not be found""}}");
        }
    }
}