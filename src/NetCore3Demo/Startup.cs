using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace NetCore3Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseGlobalExceptionHandler(x =>
            {
                x.ContentType = "application/json";
                x.OnError((ex, c) =>
                {
                    
                    return Task.CompletedTask;
                });
/*
                x.ResponseBody(s => JsonConvert.SerializeObject(new
                {
                    Message = "An error occured whilst processing your request"
                }));
*/
                x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
                    .WithBody((ex, context) => JsonConvert.SerializeObject(new
                    {
                        Message = "Record could not be found"
                    }));
            });

            app.Map("/error", x => x.Run(y => throw new HttpRequestException("boom")));

/*
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
*/
        }
    }
}