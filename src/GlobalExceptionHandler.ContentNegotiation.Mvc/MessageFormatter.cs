using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.ContentNegotiation.Mvc
{
    public static class MessageFormatters
    {
        public static void UsingMessageFormatter<T>(this IHandledFormatters formatter, T response)
        {
            Task Formatter(Exception x, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(response);
                return Task.CompletedTask;
            }

            formatter.UsingMessageFormatter(Formatter);
        }
        
        public static void UsingMessageFormatter<T>(this IHandledFormatters formatter, Func<Exception, T> f)
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f(e));
                return Task.CompletedTask;
            }

            formatter.UsingMessageFormatter(Formatter);
        }
        
        public static void UsingMessageFormatter<T>(this IHandledFormatters formatter, Func<Exception, HttpContext, T> f)
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f(e, c));
                return Task.CompletedTask;
            }

            formatter.UsingMessageFormatter(Formatter);
        }
        
        public static void UsingMessageFormatter<T>(this IHandledFormatters formatter, Func<Exception, HttpContext, HandlerContext, T> f)
        {
            Task Formatter(Exception e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteAsyncObject(f(e, c, b));
                return Task.CompletedTask;
            }

            formatter.UsingMessageFormatter(Formatter);
        }

    }
}