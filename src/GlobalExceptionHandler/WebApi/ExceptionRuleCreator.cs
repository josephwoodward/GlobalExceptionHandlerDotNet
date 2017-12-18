using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public interface IHasStatusCode
    {
        IHandledFormatters ReturnStatusCode(int statusCode);
    }

    public class ExceptionRuleCreator : IHasStatusCode, IHandledFormatters
    {
        private readonly IDictionary<Type, ExceptionConfig> _configurations;
        private readonly Type _currentFluentlyConfiguredType;

        public ExceptionRuleCreator(IDictionary<Type, ExceptionConfig> configurations, Type currentFluentlyConfiguredType)
        {
            _configurations = configurations;
            _currentFluentlyConfiguredType = currentFluentlyConfiguredType;
        }

        public IHandledFormatters ReturnStatusCode(int statusCode)
        {
            var exceptionConfig = new ExceptionConfig
            {
                StatusCode = statusCode
            };

            _configurations.Add(_currentFluentlyConfiguredType, exceptionConfig);

            return this;
        }

        public void UsingMessageFormatter(Func<Exception, HttpContext, string> formatter)
        {
            Task Formatter(Exception x, HttpContext y, HandlerContext b)
            {
                var s = formatter.Invoke(x, y);
                y.Response.WriteAsync(s);
                return Task.CompletedTask;
            }

            UsingMessageFormatter(Formatter);
        }

        public void UsingMessageFormatter(Func<Exception, HttpContext, Task> formatter)
        {
            if (formatter == null)
                throw new NullReferenceException(nameof(formatter));

            Task Formatter(Exception x, HttpContext y, HandlerContext b)
            {
                formatter.Invoke(x, y);
                return Task.CompletedTask;
            }

            UsingMessageFormatter(Formatter);
        }
        
        public void UsingMessageFormatter(Func<Exception, string, Task> formatter)
        {
            SetMessageFormatter((exception, context, arg3) => Task.CompletedTask);
        }

        public void UsingMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
        {
            SetMessageFormatter(formatter);
        }

        private void SetMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
        {
            if (formatter == null)
                throw new NullReferenceException(nameof(formatter));

	        ExceptionConfig exceptionConfig = _configurations[_currentFluentlyConfiguredType];
	        exceptionConfig.Formatter = formatter;
        }
    }
}