using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public interface IHasStatusCode
    {
        IHasMessageFormatter ReturnStatusCode(HttpStatusCode statusCode);
    }

    public interface IHasMessageFormatter
    {
        void UsingMessageFormatter(Func<Exception, HttpContext, Task> formatter);
    }

    public class ExceptionRuleCreator : IHasStatusCode, IHasMessageFormatter
    {
	    readonly IDictionary<Type, ExceptionConfig> _configurations;
	    readonly Type _currentFluentlyConfiguredType;

        public ExceptionRuleCreator(IDictionary<Type, ExceptionConfig> configurations, Type currentFluentlyConfiguredType)
        {
            _configurations = configurations;
            _currentFluentlyConfiguredType = currentFluentlyConfiguredType;
        }

        public IHasMessageFormatter ReturnStatusCode(HttpStatusCode statusCode)
        {
            var exceptionConfig = new ExceptionConfig
            {
                StatusCode = statusCode
            };

            _configurations.Add(_currentFluentlyConfiguredType, exceptionConfig);

            return this;
        }

        public void UsingMessageFormatter(Func<Exception, HttpContext, Task> formatter)
        {
			if (formatter == null) throw new ArgumentNullException(nameof(formatter));

	        var exceptionConfig = _configurations[_currentFluentlyConfiguredType];
	        exceptionConfig.Formatter = formatter;
        }
    }
}