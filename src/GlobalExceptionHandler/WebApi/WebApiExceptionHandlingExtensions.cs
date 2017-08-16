using System;
using GlobalExceptionHandler.WebApi;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebApiExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseWebApiGlobalExceptionHandler(this IApplicationBuilder app, Action<WebApiExceptionHandlingConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return app.UseMiddleware<WebApiExceptionHandlingMiddleware>(configuration);
        }
    }
}