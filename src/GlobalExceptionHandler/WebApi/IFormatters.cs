using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    /* Important: Keep these base contract signatures the same for consistency */

    public interface IHandledFormatters
    {
        void WithBody(Func<Exception, HttpContext, string> formatter);
        
        void WithBody(Func<Exception, HttpContext, Task> formatter);
        
        void WithBody(Func<Exception, HttpContext, HandlerContext, Task> formatter);
        
        [Obsolete("UsingMessageFormatter(..) is obsolete and will be removed soon, use WithBody(..) instead", false)]
        void UsingMessageFormatter(Func<Exception, HttpContext, string> formatter);

        [Obsolete("UsingMessageFormatter(..) is obsolete and will be removed soon, use WithBody(..) instead", false)]
        void UsingMessageFormatter(Func<Exception, HttpContext, Task> formatter);

        [Obsolete("UsingMessageFormatter(..) is obsolete and will be removed soon, use WithBody(..) instead", false)]
        void UsingMessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter);
    }

    public interface IUnhandledFormatters
    {
        [Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use DefaultResponseBody(..) instead", false)]
        void MessageFormatter(Func<Exception, HttpContext, string> formatter);

        [Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use DefaultResponseBody(..) instead", false)]
        void MessageFormatter(Func<Exception, HttpContext, Task> formatter);

        [Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use DefaultResponseBody(..) instead", false)]
        void MessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter);
        
        void DefaultResponseBody(Func<Exception, HttpContext, string> formatter);

        void DefaultResponseBody(Func<Exception, HttpContext, Task> formatter);

        void DefaultResponseBody(Func<Exception, HttpContext, HandlerContext, Task> formatter);
    }
}