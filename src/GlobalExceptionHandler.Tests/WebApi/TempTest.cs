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
    public class TempTest : IClassFixture<WebApiServerFixture>
    {
        private readonly IWebHostBuilder _webHost;
        private const string Path = "/api/productnotfound";
        public TempTest(WebApiServerFixture fixture)
        {
            _webHost = fixture.CreateWebHost();
            _webHost.Configure(app => {

                app.UseWebApiGlobalExceptionHandler(x => {
                    x.ContentType = "application/json";
                    x.ForException<ProductNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
                });

                app.Map(Path, config => {
                    config.Run(context => {
                        throw new ProductNotFoundException();
                    });
                });

            });
        }

        [Fact]
        public async Task Test1Async()
        {
            // Act
            var server = new TestServer(_webHost);
            using(var client = server.CreateClient()){
                var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), Path);
                var responseMessage = await client.SendAsync(requestMessage);
                var content = await responseMessage.Content.ReadAsStringAsync();

                // Assert
                responseMessage.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
                responseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                content.ShouldContain(nameof(ProductNotFoundException));
            }
        }
    }
}