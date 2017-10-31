using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

        void UsingMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter);
    }

    public class ExceptionRuleCreator : IHasStatusCode, IHasMessageFormatter
    {
        private readonly IDictionary<Type, ExceptionConfig> _configurations;
        private readonly Type _currentFluentlyConfiguredType;

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

        public void UsingMessageFormatter(Func<Exception, HttpContext> formatter)
        {
            Task Func(Exception exception, HttpContext context, HandlerContext arg3) => formatter.Invoke(exception, context);
            UsingMessageFormatter(Func);
        }

        public void UsingMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
        {
            Func<Exception, HttpContext, HandlerContext, Task> res = (e, h, c) =>
            {
                return 
            };
            SetMessageFormatter(res);
        }

        private void SetMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
        {
			if (formatter == null)
			    throw new ArgumentNullException(nameof(formatter));

	        ExceptionConfig exceptionConfig = _configurations[_currentFluentlyConfiguredType];
	        exceptionConfig.Formatter = formatter;
        }
    }
}