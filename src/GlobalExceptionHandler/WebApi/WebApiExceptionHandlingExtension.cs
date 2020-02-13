using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GlobalExceptionHandler.WebApi
{
    public static class WebApiExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => UseGlobalExceptionHandler(app, configuration => {});

        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration)
            => app.UseGlobalExceptionHandler(configuration, NullLoggerFactory.Instance);

        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var options = new ExceptionHandlerOptions().SetHandler(configuration, loggerFactory);
            return app.UseMiddleware<ExceptionHandlerMiddleware>(Options.Create(options), NullLoggerFactory.Instance);
        }
    }
}