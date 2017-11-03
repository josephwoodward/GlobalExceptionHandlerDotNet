using System;
using System.Net;
using GlobalExceptionHandler.WebApi;
using GlobalExceptionHandlerDotNet.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

/*
            app.UseWebApiGlobalExceptionHandler(x =>
            {
                x.ForException<ArgumentException>()
                    .ReturnStatusCode(HttpStatusCode.Conflict)
                    .UsingMessageFormatter((e, c) => c.WriteAsyncObject(new DemoOutput
                    {
                        Message = e.Message
                    }));
            });
*/
            app.UseExceptionHandler().WithConventions(x =>
            {
                x.ForException<ArgumentException>().ReturnStatusCode(HttpStatusCode.Conflict).UsingMessageFormatter((e, c) => c.WriteAsyncObject(new DemoOutput
                    {
                        Message = e.Message
                    }));
            });

            /*app.UseExceptionHandler(new ExceptionHandlerOptions().SetHandler(x =>
            {
                x.ForException<ArgumentException>().ReturnStatusCode(HttpStatusCode.Conflict)
                    .UsingMessageFormatter((e, c) => c.WriteAsyncObject(new DemoOutput
                    {
                        Message = e.Message
                    }));
            }));*/

            app.UseMvc();
        }
    }

    public class DemoOutput
    {
        public string Message { get; set; }
    }
}