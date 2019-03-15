using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public interface IHasStatusCode
    {
        [Obsolete("ReturnStatusCode(..) is obsolete and will be removed soon, use ToStatusCode(..) instead", false)]
        IHandledFormatters ReturnStatusCode(int statusCode);
    }

    public interface IHasStatusCode<out TException> where TException: Exception
    {
        IHandledFormatters<TException> ToStatusCode(int statusCode);
        IHandledFormatters<TException> ToStatusCode(HttpStatusCode statusCode);
        IHandledFormatters<TException> ToStatusCode(Func<TException, int> statusCodeResolver);
        IHandledFormatters<TException> ToStatusCode(Func<TException, HttpStatusCode> statusCodeResolver);
    }

    internal class ExceptionRuleCreator : IHasStatusCode, IHandledFormatters
    {
        private readonly IDictionary<Type, ExceptionConfig> _configurations;
        private readonly Type _currentFluentlyConfiguredType;

        public ExceptionRuleCreator(IDictionary<Type, ExceptionConfig> configurations, Type currentFluentlyConfiguredType)
        {
            _configurations = configurations;
            _currentFluentlyConfiguredType = currentFluentlyConfiguredType;
        }

        public IHandledFormatters ReturnStatusCode(int statusCode)
            => ToStatusCode(statusCode);

         IHandledFormatters ToStatusCode(int statusCode)
            => ToStatusCode(ex => statusCode);

         IHandledFormatters ToStatusCode(Func<Exception, int> statusCodeResolver)
        {
            var exceptionConfig = new ExceptionConfig
            {
                StatusCodeResolver = statusCodeResolver
            };

            _configurations.Add(_currentFluentlyConfiguredType, exceptionConfig);

            return this;
        }

        public void UsingMessageFormatter(Func<Exception, HttpContext, string> formatter)
            => WithBody(formatter);

        public void UsingMessageFormatter(Func<Exception, HttpContext, Task> formatter)
            => WithBody(formatter);

        public void UsingMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
            => WithBody(formatter);

        private void WithBody(Func<Exception, HttpContext, string> formatter)
        {
            Task Formatter(Exception x, HttpContext y, HandlerContext b)
            {
                var s = formatter.Invoke(x, y);
                return y.Response.WriteAsync(s);
            }

            UsingMessageFormatter(Formatter);
        }

        private void WithBody(Func<Exception, HttpContext, Task> formatter)
        {
            if (formatter == null)
                throw new NullReferenceException(nameof(formatter));

            Task Formatter(Exception x, HttpContext y, HandlerContext b)
            {
                return formatter.Invoke(x, y);
            }

            UsingMessageFormatter(Formatter);
        }

        private void WithBody(Func<Exception, HttpContext, HandlerContext, Task> formatter)
            => SetMessageFormatter(formatter);

        private void SetMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
        {
            if (formatter == null)
                throw new NullReferenceException(nameof(formatter));

	        var exceptionConfig = _configurations[_currentFluentlyConfiguredType];
	        exceptionConfig.Formatter = formatter;
        }
    }

    internal class ExceptionRuleCreator<TException> : IHasStatusCode<TException>, IHandledFormatters<TException> where TException: Exception
    {
        private readonly IDictionary<Type, ExceptionConfig> _configurations;

        public ExceptionRuleCreator(IDictionary<Type, ExceptionConfig> configurations)
        {
            _configurations = configurations;
        }

        public IHandledFormatters<TException> ToStatusCode(int statusCode)
            => ToStatusCodeImpl(ex => statusCode);

        public IHandledFormatters<TException> ToStatusCode(HttpStatusCode statusCode)
            => ToStatusCodeImpl(ex => (int)statusCode);

        public IHandledFormatters<TException> ToStatusCode(Func<TException, int> statusCodeResolver)
            => ToStatusCodeImpl(statusCodeResolver);

        public IHandledFormatters<TException> ToStatusCode(Func<TException, HttpStatusCode> statusCodeResolver)
            => ToStatusCodeImpl(x => (int)statusCodeResolver(x));

        private IHandledFormatters<TException> ToStatusCodeImpl(Func<TException, int> statusCodeResolver)
        {
            int wrappedResolver(Exception x) => statusCodeResolver((TException)x);
            var exceptionConfig = new ExceptionConfig
            {
                StatusCodeResolver = wrappedResolver
            };

            _configurations.Add(typeof(TException), exceptionConfig);

            return this;
        }

        public void WithBody(Func<TException, HttpContext, string> formatter)
        {
            Task Formatter(TException x, HttpContext y, HandlerContext b)
            {
                var s = formatter.Invoke(x, y);
                return y.Response.WriteAsync(s);
            }

            UsingMessageFormatter(Formatter);
        }

        public void WithBody(Func<TException, HttpContext, Task> formatter)
        {
            if (formatter == null)
                throw new NullReferenceException(nameof(formatter));

            Task Formatter(TException x, HttpContext y, HandlerContext b)
            {
                return formatter.Invoke(x, y);
            }

            UsingMessageFormatter(Formatter);
        }

        public void UsingMessageFormatter(Func<TException, HttpContext, HandlerContext, Task> formatter)
            => WithBody(formatter);


        public void WithBody(Func<TException, HttpContext, HandlerContext, Task> formatter)
            => SetMessageFormatter(formatter);

        private void SetMessageFormatter(Func<TException, HttpContext, HandlerContext, Task> formatter)
        {
            if (formatter == null)
                throw new NullReferenceException(nameof(formatter));

            Task wrappedFormatter(Exception x, HttpContext y, HandlerContext z) => formatter((TException)x, y, z);
	        var exceptionConfig = _configurations[typeof(TException)];
	        exceptionConfig.Formatter = wrappedFormatter;
        }
    }
}