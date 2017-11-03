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
    public class ContentNegotiationXml : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;

        public ContentNegotiationXml(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            
            var webHost = fixture.CreateWebHost();

            webHost.Configure(app =>
            {
                app.UseWebApiGlobalExceptionHandler(x =>
                {
                    x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound)
                        .UsingMessageFormatter((e, c, h) => c.WriteAsyncObject(new TestResponse
                        {
                            Message = "An exception occured"
                        }));
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
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                _response = client.SendAsync(requestMessage).Result;
            }
        }
        
        [Fact]
        public void Returns_correct_response_type()
        {
            _response.Content.Headers.ContentType.MediaType.ShouldBe("text/xml");
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
            content.ShouldContain("<Message>An exception occured</Message>");
        }
    }
}