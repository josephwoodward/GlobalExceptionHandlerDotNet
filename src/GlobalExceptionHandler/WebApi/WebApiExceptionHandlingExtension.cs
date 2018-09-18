using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlobalExceptionHandler.WebApi
{
    public static class WebApiExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => UseGlobalExceptionHandler(app, configuration => {});
        
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var opts = new ExceptionHandlerOptions {ExceptionHandler = ctx => Task.CompletedTask};
            opts.SetHandler(configuration);
            
            return app.UseExceptionHandler(opts);
        }

        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            return app.UseMiddleware<ExceptionHandlerMiddleware>(Options.Create(new ExceptionHandlerOptions().SetHandler(configuration)), loggerFactory);
        }

        [Obsolete("app.UseExceptionHandler().WithConventions(..) is obsolete, please use app.UseGlobalExceptionHandler(..) instead", true)]
        public static IApplicationBuilder WithConventions(this IApplicationBuilder app)
            => WithConventions(app, configuration => {});

        [Obsolete("app.UseExceptionHandler().WithConventions(..) is obsolete, please use app.UseGlobalExceptionHandler(..) instead", true)]
        public static IApplicationBuilder WithConventions(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var opts = new ExceptionHandlerOptions();
            opts.SetHandler(configuration);
            
            return app.UseExceptionHandler(opts);
        }

        [Obsolete("app.UseExceptionHandler().WithConventions(..) is obsolete, please use app.UseGlobalExceptionHandler(..) instead", true)]
        public static IApplicationBuilder WithConventions(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            return app.UseMiddleware<ExceptionHandlerMiddleware>(Options.Create(new ExceptionHandlerOptions().SetHandler(configuration)), loggerFactory);
        }
    }
}