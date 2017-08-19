using System.Threading.Tasks;
using GlobalExceptionHandler.Tests.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalExceptionHandler.Tests.WebApi
{
        public class StartupWebApi
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // app.UseWebApiGlobalExceptionHandler(x =>
            // {
            //     x.ContentType = "application/json";
            //     x.ForException<ProductNotFoundException>().ReturnStatusCode(System.Net.HttpStatusCode.NotModified);
            // });

            // app.UseMvc();
        }
    }
}