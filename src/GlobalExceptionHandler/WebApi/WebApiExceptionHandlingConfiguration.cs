using System;
using System.Collections.Concurrent;

namespace GlobalExceptionHandler.WebApi
{
    public class WebApiExceptionHandlingConfiguration
    {
        public string ContentType { get; set; }
        private readonly IExceptionConfig _defaultExceptionConfig;
        private readonly ConcurrentDictionary<Type, IExceptionConfig> _configuration;

        public WebApiExceptionHandlingConfiguration()
        {
            _configuration = new ConcurrentDictionary<Type, IExceptionConfig>();
            _defaultExceptionConfig = new DefaultExceptionConfig();
        }

        public IHasStatusCode ForException<T>() where T : Exception
        {
            var type = typeof(T);
            return new ExceptionRuleCreator(_configuration, type);
        }

        public void MessageFormatter(Func<Exception, string> formatter)
        {
            // Override default global formatter with user specified formatter
            _defaultExceptionConfig.Formatter = formatter;
/*
            foreach (var config in Configuration)
            {
                if (config.Value.Formatter == null)
                {
                    config.Value.Formatter = formatter;
                } 
            }
*/
        }

        public ConcurrentDictionary<Type, IExceptionConfig> BuildOptions()
        {
            return _configuration;
        }

        public IExceptionConfig ExceptionConfig => _defaultExceptionConfig;
    }
}