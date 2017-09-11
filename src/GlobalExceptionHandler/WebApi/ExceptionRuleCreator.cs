using System;
using System.Collections.Concurrent;
using System.Net;

namespace GlobalExceptionHandler.WebApi
{
    public interface IHasStatusCode
    {
        IHasMessageFormatter ReturnStatusCode(HttpStatusCode statusCode);
    }

    public interface IHasMessageFormatter
    {
        void UsingFormatter(Func<Exception, string> formatter);
    }

    public class ExceptionRuleCreator : IHasStatusCode, IHasMessageFormatter
    {
        private readonly ConcurrentDictionary<Type, IExceptionConfig> _configurations;
        private readonly Type _type;

        public ExceptionRuleCreator(ConcurrentDictionary<Type, IExceptionConfig> configurations, Type type)
        {
            _configurations = configurations;
            _type = type;
        }

        public IHasMessageFormatter ReturnStatusCode(HttpStatusCode statusCode)
        {
            var c = new ExceptionConfig
            {
                StatusCode = statusCode
            };

            _configurations.AddOrUpdate(_type, c, (type, config) => c);

            return this;
        }

        public void UsingFormatter(Func<Exception, string> formatter)
        {
            if (_configurations.TryGetValue(_type, out var exceptionConfig))
            {
                exceptionConfig.Formatter = formatter;
                _configurations.TryUpdate(_type, exceptionConfig, exceptionConfig);
            }
        }
    }
}