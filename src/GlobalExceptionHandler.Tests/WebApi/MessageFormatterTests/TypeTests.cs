using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GlobalExceptionHandler.ContentNegotiation;
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
    public class TypeTests : IClassFixture<WebApiServerFixture>, IAsyncLifetime
    {
	    private const string ApiProductNotFound = "/api/productnotfound";
	    private readonly HttpClient _client;
	    private readonly HttpRequestMessage _requestMessage;
	    private HttpResponseMessage _response;

	    public TypeTests(WebApiServerFixture fixture)
        {
            // Arrange
	        var webHost = fixture.CreateWebHostWithXmlFormatters();
            webHost.Configure(app =>
            {
	            app.UseGlobalExceptionHandler(x =>
	            {
					x.Map<BaseException>()
						.ToStatusCode(StatusCodes.Status502BadGateway)
						.WithBody((e, c, h) => c.Response.WriteAsync("<Message>Not Thrown Message</Message>"));

		            x.Map<Level1ExceptionA>()
			            .ToStatusCode(StatusCodes.Status409Conflict)
			            .WithBody(new TestResponse
			            {
				            Message = "Conflict"
			            });

		            x.Map<Level1ExceptionB>()
			            .ToStatusCode(StatusCodes.Status400BadRequest)
			            .WithBody(e => new TestResponse
			            {
				            Message = "Bad Request"
			            });

		            x.Map<Level2ExceptionB>()
			            .ToStatusCode(StatusCodes.Status403Forbidden)
			            .WithBody(new TestResponse
			            {
				            Message = "Forbidden"
			            });
				});

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new Level1ExceptionB());
                });
            });
	        
	        _requestMessage = new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound);
	        _requestMessage.Headers.Accept.Clear();
	        _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

	        _client = new TestServer(webHost).CreateClient();
        }
	    
        public async Task InitializeAsync()
        {
	        _response = await _client.SendAsync(_requestMessage);
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

        public Task DisposeAsync()
			=> Task.CompletedTask;
    }

	internal class BaseException : Exception { }

	internal class Level1ExceptionA : BaseException { }

	internal class Level1ExceptionB : BaseException { }

	internal class Level2ExceptionA : Level1ExceptionA { }

	internal class Level2ExceptionB : Level1ExceptionB { }
}