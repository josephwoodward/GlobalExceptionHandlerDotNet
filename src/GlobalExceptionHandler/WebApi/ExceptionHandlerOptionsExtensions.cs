using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace GlobalExceptionHandler.WebApi
{
    internal static class ExceptionHandlerOptionsExtensions
    {
        public static ExceptionHandlerOptions SetHandler(this ExceptionHandlerOptions exceptionHandlerOptions, Action<ExceptionHandlerConfiguration> configurationAction, ILoggerFactory loggerFactory)
        {
            var configuration = new ExceptionHandlerConfiguration(ExceptionConfig.UnsafeFormatterWithDetails, loggerFactory);
            configurationAction(configuration);

            exceptionHandlerOptions.ExceptionHandler = configuration.BuildHandler();
			return exceptionHandlerOptions;
        }
    }
}