using System.Net;
using GlobalExceptionHandler.WebApi.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseWebApiGlobalExceptionHandler(x =>
            {
                x.ContentType = "application/json";
                x.ForException<RecordNotFoundException>().ReturnStatusCode(HttpStatusCode.NotFound);
            });

            app.UseMvc();
        }
    }
}