using System;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi
{
    public class HandleExceptionTests2 : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpResponseMessage _response;

        public HandleExceptionTests2(WebApiServerFixture fixture)
        {
            // Arrange
            const string requestUri = "/api/productnotfound";
            var webHost = fixture.CreateWebHost();
            webHost.Configure(app =>
            {
	            app.UseExceptionHandler().WithConventions(x =>
	            {
					x.ForException<BaseException>()
						.ReturnStatusCode(HttpStatusCode.BadGateway)
						.UsingMessageFormatter((e, c, h) => c.Response.WriteAsync("my custom message for applicaiton exceptions"));

		            x.ForException<Level1ExceptionA>()
			            .ReturnStatusCode(HttpStatusCode.Conflict)
			            .UsingMessageFormatter((e, c, h) => c.WriteAsyncObject(new TestResponse
			            {
				            Message = "Hello World 1"
			            }));

		            x.ForException<Level1ExceptionB>()
			            .ReturnStatusCode(HttpStatusCode.Ambiguous)
			            .UsingMessageFormatter((e, c, h) => c.WriteAsyncObject(new TestResponse
			            {
				            Message = "Hello World 1"
			            }));

		            x.ForException<Level2ExceptionA>()
			            .ReturnStatusCode(HttpStatusCode.ExpectationFailed)
			            .UsingMessageFormatter((e, c, h) => c.WriteAsyncObject(new TestResponse
			            {
				            Message = "Hello World 1"
			            }));

		            x.ForException<Level2ExceptionB>()
			            .ReturnStatusCode(HttpStatusCode.Forbidden)
			            .UsingMessageFormatter((e, c, h) => c.WriteAsyncObject(new TestResponse
			            {
				            Message = "Hello World 1"
			            }));
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
            _response.StatusCode.ShouldBe(HttpStatusCode.Ambiguous);
        }

        [Fact]
        public async Task Returns_correct_body()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.ShouldContain(@"<Message>Hello World 1</Message>");
        }
    }

	class BaseException : Exception { }

	class Level1ExceptionA : BaseException { }

	class Level1ExceptionB : BaseException { }

	class Level2ExceptionA : Level1ExceptionA { }

	class Level2ExceptionB : Level1ExceptionB { }
}