using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingOptionsSetup
    {
        private readonly ConcurrentDictionary<Type, IExceptionConfig> _configuration;
        private Func<Exception, string> _globalFormatter;
        private Func<Exception, HttpContext, Task> _logger;
        
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
                Logger = _logger,
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

        public void OnError(Func<Exception, HttpContext, Task> log)
        {
            _logger = log;
        }
    }
}