using System;
using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingConfiguration
    {
        private ConcurrentDictionary<Type, HttpStatusCode> Configuration { get; }
        
        public string ContentType { get; set; }
        public Func<Exception, string> ExceptionFormatter { get; private set; }

        public WebApiExceptionHandlingConfiguration()
        {
            Configuration = new ConcurrentDictionary<Type, HttpStatusCode>();
            ExceptionFormatter = exception => JsonConvert.SerializeObject(new
            {
                error = new
                {
                    exception = exception.GetType().Name,
                    message = exception.Message
                }
            });
        }

        public IHasStatusCode ForException<T>() where T : Exception
        {
            var type = typeof(T);
            return new StatusCode(Configuration, type);
        }

        public void MessageFormatter(Func<Exception, string> formatter)
        {
            ExceptionFormatter = formatter;
        }
        
        public ConcurrentDictionary<Type, HttpStatusCode> BuildOptions()
        {
            return Configuration;
        }
    }
}