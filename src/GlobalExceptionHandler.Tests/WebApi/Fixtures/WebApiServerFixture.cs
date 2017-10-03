using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalExceptionHandler.Tests.WebApi.Fixtures
{
    public class WebApiServerFixture
    {
        public IWebHostBuilder CreateWebHost()
        {
            var config = new ConfigurationBuilder().Build();
            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .ConfigureServices(services => services.AddMvc());

            return host;
        }
    }
}