using System;
using GlobalExceptionHandler;
using GlobalExceptionHandler.Mvc;
using GlobalExceptionHandler.WebApi;

namespace Microsoft.AspNetCore.Builder
{
    public static class MvcExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseMvcGlobalExceptionHandler(this IApplicationBuilder app, Action<ExceptionHandlingConfiguration> configuration)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return app.UseMiddleware<MvcExceptionHandlingMiddleware>(configuration);
        }
    }
}