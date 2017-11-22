using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GlobalExceptionHandler.ContentNegotiation.Mvc;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.MessageFormatterTests
{
    public class TypeTests : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;

        public TypeTests(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHostWithXmlFormatters();
            webHost.Configure(app =>
            {
	            app.UseExceptionHandler().WithConventions(x =>
	            {
					x.ForException<BaseException>()
						.ReturnStatusCode(HttpStatusCode.BadGateway)
						.UsingMessageFormatter((e, c, h) => c.Response.WriteAsync("<Message>Not Thrown Message</Message>"));

		            x.ForException<Level1ExceptionA>()
			            .ReturnStatusCode(HttpStatusCode.Conflict)
			            .UsingMessageFormatter(new TestResponse
			            {
				            Message = "Conflict"
			            });

		            x.ForException<Level1ExceptionB>()
			            .ReturnStatusCode(HttpStatusCode.BadRequest)
			            .UsingMessageFormatter(e => new TestResponse
			            {
				            Message = "Bad Request"
			            });

		            x.ForException<Level2ExceptionB>()
			            .ReturnStatusCode(HttpStatusCode.Forbidden)
			            .UsingMessageFormatter(new TestResponse
			            {
				            Message = "Forbidden"
			            });
				});

                app.Map(requestUri, config =>
                {
                    config.Run(context => throw new Level1ExceptionB());
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
            _response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldContain(@"<Message>Bad Request</Message>");
        }
    }

	internal class BaseException : Exception { }

	internal class Level1ExceptionA : BaseException { }

	internal class Level1ExceptionB : BaseException { }

	internal class Level2ExceptionA : Level1ExceptionA { }

	internal class Level2ExceptionB : Level1ExceptionB { }
}