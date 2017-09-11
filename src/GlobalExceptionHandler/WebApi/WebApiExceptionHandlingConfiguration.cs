using System;
using System.Collections.Concurrent;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingConfiguration
    {
        private Func<Exception, string> _globalFormatter;
        public string ContentType { get; set; }
        private readonly ConcurrentDictionary<Type, IExceptionConfig> _configuration;

        public WebApiExceptionHandlingConfiguration(Func<Exception, string> globalFormatter)
        {
            _configuration = new ConcurrentDictionary<Type, IExceptionConfig>();
            _globalFormatter = globalFormatter;
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

        public ConcurrentDictionary<Type, IExceptionConfig> BuildOptions()
        {
            return _configuration;
        }

        public Func<Exception, string> GlobalFormatter => _globalFormatter;
    }
}