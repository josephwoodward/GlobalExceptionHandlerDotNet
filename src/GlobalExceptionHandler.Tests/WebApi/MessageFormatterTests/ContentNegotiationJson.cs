using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using GlobalExceptionHandler.Tests.WebApi.Fixtures;
using GlobalExceptionHandler.WebApi;
using GlobalExceptionHandlerDotNet.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.MessageFormatterTests
{
    public class ContentNegotiationJson : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;
        private const string ContentType = "application/json";

        public ContentNegotiationJson(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
                app.UseExceptionHandler().WithConventions(x =>
                {
                    x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound)
                        .UsingMessageFormatter(new TestResponse
                        {
                            Message = "An exception occured"
                        });
                });

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new RecordNotFoundException("Record could not be found"));
                });
            });

            // Act
            var server = new TestServer(webHost);
            using (var client = server.CreateClient())
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), requestUri);
                requestMessage.Headers.Accept.Clear();
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                _response = client.SendAsync(requestMessage).Result;
            }
        }
        
        [Fact]
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe(ContentType);
        }

        [Fact]
        public void Returns_correct_status_code()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldContain("{\"message\":\"An exception occured\"}");
        }
    }
}