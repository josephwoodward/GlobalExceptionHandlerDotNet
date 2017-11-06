using System;
using Microsoft.AspNetCore.Builder;

namespace GlobalExceptionHandler.WebApi
{
    internal static class ExceptionHandlerOptionsExtensions
    {
        public static ExceptionHandlerOptions SetHandler(this ExceptionHandlerOptions exceptionHandlerOptions, Action<ExceptionHandlerConfiguration> configurationAction)
        {
            var configuration = new ExceptionHandlerConfiguration(ExceptionConfig.UnsafeFormatterWithDetails);
            configurationAction(configuration);

            exceptionHandlerOptions.ExceptionHandler = configuration.BuildHandler();
			return exceptionHandlerOptions;
        }
    }
}