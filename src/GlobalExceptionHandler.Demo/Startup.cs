using System;
using System.Net;
using GlobalExceptionHandler.ContentNegotiation.Mvc;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Map("/demo", x => x.Run(y => throw new ArgumentException("boo!")));

            app.UseExceptionHandler().WithConventions(x => x.MessageFormatter((e, c) => JsonConvert.SerializeObject(new
            {
                Message = "Hello World!"
            })));
        }
    }

    public class DemoOutput
    {
        public string Message { get; set; }
    }
}