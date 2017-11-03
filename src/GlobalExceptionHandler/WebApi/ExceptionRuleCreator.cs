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
        void UsingMessageFormatter(Func<Exception, HttpContext, string> formatter);
        
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