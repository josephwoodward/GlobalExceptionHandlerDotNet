using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.ContentNegotiation
{
    public static class MessageFormatters
    {
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

        public static void WithBody<T, TException>(this IHandledFormatters<TException> formatter, Func<TException, T> f) where TException: Exception
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f((TException)e));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }

        public static void UsingMessageFormatter<T, TException>(this IHandledFormatters<TException> formatter, Func<TException, HttpContext, T> f) where TException : Exception
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f((TException)e, c));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
        
        public static void UsingMessageFormatter<T, TException>(this IHandledFormatters<TException> formatter, Func<TException, HttpContext, HandlerContext, T> f) where TException : Exception
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f((TException)e, c, b));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
    }
}