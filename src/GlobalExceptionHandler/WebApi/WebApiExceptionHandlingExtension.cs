using System;
using Microsoft.AspNetCore.Builder;

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
            return WithConventions(app, configuration => { });
        }

        public static IApplicationBuilder WithConventions(this IApplicationBuilder app, Action<ExceptionHandlerConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return app.UseExceptionHandler(new ExceptionHandlerOptions().SetHandler(configuration));
        }
    }
}