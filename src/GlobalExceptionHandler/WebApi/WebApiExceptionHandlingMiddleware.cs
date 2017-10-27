using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
/*public class WebApiExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<ExceptionHandlerConfiguration> _setOptions;

        private OptionsContainer _optionsContainer;
        private string _contentType = "application/json"; // Default content type

        public WebApiExceptionHandlingMiddleware(RequestDelegate next, Action<ExceptionHandlerConfiguration> setOptions)
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

                if (_optionsContainer.Logger != null)
                {
                    await _optionsContainer.Logger(ex, context);
                }

                await WriteExceptionAsync(context, ex, settings).ConfigureAwait(false);
            }
        }

        private ExceptionConfig GetHandlerSettings(Type exceptionType)
        {
            if (_optionsContainer.Exceptions.TryGetValue(exceptionType, out ExceptionConfig context))
            {
                context.Formatter = context.Formatter ?? _optionsContainer.GlobalFormatter;
                context.StatusCode = context.StatusCode;

                return context;
            }
            
            return new ExceptionConfig
            {
                Formatter = _optionsContainer.GlobalFormatter,
                StatusCode = HttpStatusCode.InternalServerError
            };
        }

        private async Task WriteExceptionAsync(HttpContext context, Exception exception, ExceptionConfig exceptionParams)
        {
            var response = context.Response;
            response.ContentType = _contentType;
            response.StatusCode = (int) (exceptionParams.StatusCode ?? HttpStatusCode.InternalServerError);
            var message = exceptionParams.Formatter(exception);

            await response.WriteAsync(message).ConfigureAwait(false);
        }
    }*/}