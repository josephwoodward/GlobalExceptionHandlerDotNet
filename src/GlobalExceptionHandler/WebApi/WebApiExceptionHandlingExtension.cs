using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlobalExceptionHandler.WebApi
{
    public static class WebApiExceptionHandlingExtensions
    {
        [Obsolete("UseWebApiGlobalExceptionHandler is obsolete, use app.UseExceptionHandler().WithConventions(..) instead", true)]
        public static IApplicationBuilder UseWebApiGlobalExceptionHandler(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return app.UseExceptionHandler(new ExceptionHandlerOptions().SetHandler(configuration));
        }

        public static IApplicationBuilder WithConventions(this IApplicationBuilder app)
        {
            return WithConventions(app, configuration =>
            {
                
            });
        }

        public static IApplicationBuilder WithConventions(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var opts = new ExceptionHandlerOptions()/*{ExceptionHandler = ctx => Task.CompletedTask}*/;
            opts.SetHandler(configuration);
            
            return app.UseExceptionHandler(opts);
        }

        public static IApplicationBuilder WithConventions(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            return app.UseMiddleware<ExceptionHandlerMiddleware>(
                Options.Create(new ExceptionHandlerOptions().SetHandler(configuration)), 
                loggerFactory);
        }
    }
}