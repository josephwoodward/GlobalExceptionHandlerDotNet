using System;
using System.Collections.Concurrent;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingConfiguration
    {
        public string ContentType { get; set; }
        private readonly ConcurrentDictionary<Type, IExceptionConfig> _configuration;

        public WebApiExceptionHandlingConfiguration(Func<Exception, string> globalFormatter)
        {
            _configuration = new ConcurrentDictionary<Type, IExceptionConfig>();
            GlobalFormatter = globalFormatter;
        }

        public IHasStatusCode ForException<T>() where T : Exception
        {
            var type = typeof(T);
            return new ExceptionRuleCreator(_configuration, type);
        }

        public void MessageFormatter(Func<Exception, string> formatter)
        {
            GlobalFormatter = formatter;
        }

        internal ConcurrentDictionary<Type, IExceptionConfig> BuildOptions()
        {
            return _configuration;
        }

        internal Func<Exception, string> GlobalFormatter { get; private set; }
    }
}