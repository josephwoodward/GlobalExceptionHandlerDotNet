using System;
using System.Net;
using GlobalExceptionHandler.ProblemDetails.Mvc;
using GlobalExceptionHandler.Tests.WebApi.ProblemDetailsTests;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalExceptionHandler.ProblemDetails
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            /*services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://asp.net/core",
                        Detail = "Please refer to the errors property for additional details."
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = {"application/problem+json", "application/problem+xml"}
                    };
                };
            })*/;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

app.UseGlobalExceptionHandler(x =>
{
    x.Map<ArgumentException>().ToStatusCode(HttpStatusCode.BadRequest).WithProblemDetails(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Type = ex.GetType().Name,
        Detail = ex.Message,
        Title = "My details",
        Instance = "My instance",
        Status = (int)HttpStatusCode.BadRequest
    });
});

            /*app.UseHttpsRedirection();*/
            app.UseMvc();
        }
    }
}