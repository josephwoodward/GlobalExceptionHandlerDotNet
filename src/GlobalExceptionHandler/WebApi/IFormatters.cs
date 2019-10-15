using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    /* Important: Keep these base contract signatures the same for consistency */

    public interface IHandledFormatters<out TException> where TException : Exception
    {
        void WithBody(Func<TException, HttpContext, string> formatter);

        void WithBody(Func<TException, HttpContext, Task> formatter);

        void WithBody(Func<TException, HttpContext, HandlerContext, Task> formatter);
    }

    public interface IUnhandledFormatters<out TException> where TException: Exception
    {
        void ResponseBody(Func<TException, HttpContext, string> formatter);

        void ResponseBody(Func<TException, HttpContext, Task> formatter);

        void ResponseBody(Func<TException, HttpContext, HandlerContext, Task> formatter);
    }
}