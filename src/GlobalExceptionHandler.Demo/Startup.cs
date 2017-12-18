using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler().WithConventions(x => {
                x.ContentType = "application/json";
                x.MessageFormatter(s => JsonConvert.SerializeObject(new
                {
                    Message = "An error occured whilst processing your request"
                }));
                x.ForException<RecordNotFoundException>().ReturnStatusCode(StatusCodes.Status404NotFound)
                    .UsingMessageFormatter((ex, context) => JsonConvert.SerializeObject(new
                    {
                        Message = "Record could not be found"
                    }));
            });
            
            app.Map("/error", x => x.Run(y => throw new RecordNotFoundException()));
        }
    }

    public class DemoOutput
    {
        public string Message { get; set; }
    }
}