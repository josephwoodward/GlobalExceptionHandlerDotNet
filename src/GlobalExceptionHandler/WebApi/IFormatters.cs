using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    /* Important: Keep these base contract signatures the same for consistency */

    public interface IHandledFormatters
    {
        void UsingMessageFormatter(Func<Exception, HttpContext, string> formatter);

        void UsingMessageFormatter(Func<Exception, HttpContext, Task> formatter);

        void UsingMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter);
    }

    public interface IUnhandledFormatters
    {
        void MessageFormatter(Func<Exception, HttpContext, string> formatter);

        void MessageFormatter(Func<Exception, HttpContext, Task> formatter);

        void MessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter);
    }
}