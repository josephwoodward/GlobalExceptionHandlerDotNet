using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<WebApiExceptionHandlingOptionsSetup> _setOptions;

        private OptionsContainer _optionsContainer;
        private string _contentType = "application/json"; // Default content type

        public WebApiExceptionHandlingMiddleware(RequestDelegate next, Action<WebApiExceptionHandlingOptionsSetup> setOptions)
        {
            _next = next;
            _setOptions = setOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_optionsContainer == null)
            {
                var optionsSetup = new WebApiExceptionHandlingOptionsSetup();

                _setOptions(optionsSetup);

                _contentType = optionsSetup.ContentType;
                _optionsContainer = optionsSetup.BuildOptions();
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var settings = GetHandlerSettings(ex.GetType());
                await WriteExceptionAsync(context, ex, settings).ConfigureAwait(false);
            }
        }

        private IExceptionConfig GetHandlerSettings(Type exceptionType)
        {
            if (_optionsContainer.Exceptions.TryGetValue(exceptionType, out IExceptionConfig args))
            {
                args.Formatter = args.Formatter ?? _optionsContainer.GlobalFormatter;
                args.StatusCode = args.StatusCode ?? _optionsContainer.GlobalStatusCode;
                return args;
            }
            
            return new ExceptionConfig
            {
                Formatter = _optionsContainer.GlobalFormatter,
                StatusCode = HttpStatusCode.InternalServerError
            };
        }

        private async Task WriteExceptionAsync(HttpContext context, Exception exception, IExceptionConfig exceptionParams)
        {
            var response = context.Response;
            response.ContentType = _contentType;
            response.StatusCode = (int) (exceptionParams.StatusCode ?? HttpStatusCode.InternalServerError);
            var message = exceptionParams.Formatter(exception);

            await response.WriteAsync(message).ConfigureAwait(false);
        }
    }
}