using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GlobalExceptionHandler.Tests
{
/*
    public class UnitTest1
    {
        private IWebHostBuilder CreateWebHostBuilder(){

            var config = new ConfigurationBuilder().Build();
            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            return host;
        }

        [Fact]
        public async Task Test1Async()
        {
            var webHostBuilder = CreateWebHostBuilder();
            var server = new TestServer(webHostBuilder);

            using(var client = server.CreateClient()){
                var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/api/values/");
                var responseMessage = await client.SendAsync(requestMessage);

                var content = await responseMessage.Content.ReadAsStringAsync();

                Assert.Equal(content, "Hello World!");
            }
        }
    }
*/
}