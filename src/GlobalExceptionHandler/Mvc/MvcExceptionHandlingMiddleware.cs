using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.Mvc
{
    public class MvcExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<ExceptionHandlingConfiguration> _hydrateConfiguration;
        private IDictionary<Exception, HttpStatusCode> _exceptionCodesMapping;
        private string _contentType = "application/json";

        public MvcExceptionHandlingMiddleware(RequestDelegate next, Action<ExceptionHandlingConfiguration> hydrateConfiguration)
        {
            _next = next;
            _hydrateConfiguration = hydrateConfiguration;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_exceptionCodesMapping == null)
            {
                var config = new ExceptionHandlingConfiguration();
                _hydrateConfiguration(config);

                _contentType = config.ContentType;
                _exceptionCodesMapping = config.BuildOptions();
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
            await response.WriteAsync(JsonConvert.SerializeObject(new
            {
                error = new
                {
                    exception = exception.GetType().Name,
                    message = exception.Message
                }
            })).ConfigureAwait(false);
        }
    }
}