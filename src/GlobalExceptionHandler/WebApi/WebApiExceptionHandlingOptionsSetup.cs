using System;
using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingOptionsSetup
    {
        private readonly ConcurrentDictionary<Type, IExceptionConfig> _configuration;
        private Func<Exception, string> _globalFormatter;
        
        public string ContentType { get; set; }

        public WebApiExceptionHandlingOptionsSetup()
        {
            _configuration = new ConcurrentDictionary<Type, IExceptionConfig>();
        }

        public IHasStatusCode ForException<T>() where T : Exception
        {
            var type = typeof(T);
            return new ExceptionRuleCreator(_configuration, type);
        }

        public void MessageFormatter(Func<Exception, string> formatter)
        {
            _globalFormatter = formatter;
        }

        internal OptionsContainer BuildOptions()
        {
            return new OptionsContainer
            {
                Exceptions = _configuration,
                GlobalStatusCode = HttpStatusCode.InternalServerError,
                GlobalFormatter = _globalFormatter ?? (exception => JsonConvert.SerializeObject(new
                {
                    error = new
                    {
                        exception = exception.GetType().Name,
                        message = exception.Message
                    }
                }))
            };
        }

    }
}