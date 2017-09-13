using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<WebApiExceptionHandlingSetup> _setConfig;

        private ConcurrentDictionary<Type, IExceptionConfig> _configuration;
        private string _contentType = "application/json"; // Default content type
        private Func<Exception, string> _defaultFormatter;

        public WebApiExceptionHandlingMiddleware(RequestDelegate next, Action<WebApiExceptionHandlingSetup> setConfig)
        {
            _next = next;
            _setConfig = setConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_configuration == null)
            {
                var config = new WebApiExceptionHandlingSetup(_defaultGlobalFormatter);

                _setConfig(config);

                _contentType = config.ContentType;
                _configuration = config.BuildOptions();
                _defaultFormatter = config.GlobalFormatter;
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
                userConfig = new DefaultExceptionConfig
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };

            if (userConfig.Formatter == null)
                userConfig.Formatter = _defaultFormatter;

            await WriteExceptionAsync(context, exception, userConfig).ConfigureAwait(false);
        }

        private async Task WriteExceptionAsync(HttpContext context, Exception exception, IExceptionConfig userConfig)
        {
            var response = context.Response;
            response.ContentType = _contentType;
            response.StatusCode = (int) userConfig.StatusCode;
            var message = userConfig.Formatter(exception);

            await response.WriteAsync(message).ConfigureAwait(false);
        }

        private readonly Func<Exception, string> _defaultGlobalFormatter = exception => JsonConvert.SerializeObject(new
        {
            error = new
            {
                exception = exception.GetType().Name,
                message = exception.Message
            }
        });
    }
}