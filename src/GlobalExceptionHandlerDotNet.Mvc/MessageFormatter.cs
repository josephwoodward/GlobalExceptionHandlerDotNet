using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandlerDotNet.Mvc
{
    public static class MessageFormatters
    {
        public static void UsingMessageFormatter<T>(this IHasMessageFormatter f, T response)
        {
            Task Formatter(Exception x, HttpContext c, HandlerContext b)
            {
                c.WriteAsyncObject(response);
                return Task.CompletedTask;
            }

            f.UsingMessageFormatter(Formatter);
        }
    }
}