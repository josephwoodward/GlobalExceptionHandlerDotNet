using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.ContentNegotiation.Mvc
{
    public static class MessageFormatters
    {
        [Obsolete("UsingMessageFormatter(..) is obsolete and will be removed soon, use WithBody(..) instead", false)]
        public static void UsingMessageFormatter<T, TException>(this IHandledFormatters<TException> formatter, T response) where TException : Exception
            => WithBody(formatter, response);

        [Obsolete("UsingMessageFormatter(..) is obsolete and will be removed soon, use WithBody(..) instead", false)]
        public static void UsingMessageFormatter<T, TException>(this IHandledFormatters<TException> formatter, Func<Exception, T> f) where TException : Exception
            => WithBody(formatter, f);
        
        public static void WithBody<T,TException>(this IHandledFormatters<TException> formatter, T response) where TException: Exception
        {
            Task Formatter(Exception x, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(response);
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
        
        public static void WithBody<T, TException>(this IHandledFormatters<TException> formatter, Func<Exception, T> f) where TException: Exception
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f(e));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
        
        public static void UsingMessageFormatter<T, TException>(this IHandledFormatters<TException> formatter, Func<Exception, HttpContext, T> f) where TException : Exception
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f(e, c));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
        
        public static void UsingMessageFormatter<T, TException>(this IHandledFormatters<TException> formatter, Func<Exception, HttpContext, HandlerContext, T> f) where TException : Exception
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f(e, c, b));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
    }
}