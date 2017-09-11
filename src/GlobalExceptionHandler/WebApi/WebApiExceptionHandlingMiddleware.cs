using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<WebApiExceptionHandlingConfiguration> _setConfig;

        private ConcurrentDictionary<Type, IExceptionConfig> _configuration;
        private string _contentType = "application/json"; // Default content type
        private IExceptionConfig _defaultFormatter;

        public WebApiExceptionHandlingMiddleware(RequestDelegate next, Action<WebApiExceptionHandlingConfiguration> setConfig)
        {
            _next = next;
            _setConfig = setConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_configuration == null)
            {
                var config = new WebApiExceptionHandlingConfiguration();
                _setConfig(config);

                _contentType = config.ContentType;
                _configuration = config.BuildOptions();
                _defaultFormatter = config.ExceptionConfig;
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                await HandleExceptionAsync(context, exceptionType, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Type exceptionType, Exception exception)
        {
            if (!_configuration.TryGetValue(exceptionType, out var userConfig))
            {
                userConfig = new DefaultExceptionConfig
                {
                    StatusCode = userConfig.StatusCode,
                    Formatter = userConfig.Formatter
                };
            }

            userConfig.Formatter = userConfig.Formatter ?? _defaultFormatter.Formatter;

            await WriteExceptionAsync(context, exception, userConfig).ConfigureAwait(false);
        }

        private async Task WriteExceptionAsync(HttpContext context, Exception exception, IExceptionConfig userConfig)
        {
            HttpResponse response = context.Response;
            response.ContentType = _contentType;
            response.StatusCode = (int) userConfig.StatusCode;
            string message = userConfig.Formatter(exception);

            await response.WriteAsync(message).ConfigureAwait(false);
        }
    }
}