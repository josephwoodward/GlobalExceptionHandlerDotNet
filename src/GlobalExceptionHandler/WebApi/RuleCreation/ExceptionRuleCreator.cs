using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public interface IHasStatusCode<out TException> where TException: Exception
    {
        IHandledFormatters<TException> ToStatusCode(int statusCode);
        IHandledFormatters<TException> ToStatusCode(HttpStatusCode statusCode);
        IHandledFormatters<TException> ToStatusCode(Func<TException, int> statusCodeResolver);
        IHandledFormatters<TException> ToStatusCode(Func<TException, HttpStatusCode> statusCodeResolver);
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
            int WrappedResolver(Exception x) => statusCodeResolver((TException)x);
            var exceptionConfig = new ExceptionConfig
            {
                StatusCodeResolver = WrappedResolver
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
                => formatter.Invoke(x, y);

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

            Task WrappedFormatter(Exception x, HttpContext y, HandlerContext z) => formatter((TException)x, y, z);
	        var exceptionConfig = _configurations[typeof(TException)];
	        exceptionConfig.Formatter = WrappedFormatter;
        }
    }
}