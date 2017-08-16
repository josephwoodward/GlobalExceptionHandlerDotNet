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
        private readonly Action<WebApiExceptionHandlingConfiguration> _setConfig;
        
        private ConcurrentDictionary<Type, HttpStatusCode> _exceptionCodesMapping;
        private string _contentType = "application/json"; // Default content type
        private Func<Exception, string> _formatter;
        
        public WebApiExceptionHandlingMiddleware(RequestDelegate next, Action<WebApiExceptionHandlingConfiguration> setConfig)
        {
            _next = next;
            _setConfig = setConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_exceptionCodesMapping == null)
            {
                var config = new WebApiExceptionHandlingConfiguration();
                _setConfig(config);

                _contentType = config.ContentType;
                _exceptionCodesMapping = config.BuildOptions();
                _formatter = config.ExceptionFormatter;
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
            HttpStatusCode statusCode;
            if (!_exceptionCodesMapping.TryGetValue(exceptionType, out statusCode))
                statusCode = HttpStatusCode.InternalServerError;

            await WriteExceptionAsync(context, statusCode, exception).ConfigureAwait(false);
        }

        private async Task WriteExceptionAsync(HttpContext context, HttpStatusCode statusCode, Exception exception)
        {
            var response = context.Response;
            response.ContentType = _contentType;
            response.StatusCode = (int) statusCode;
            await response.WriteAsync(_formatter(exception)).ConfigureAwait(false);
        }
    }
}