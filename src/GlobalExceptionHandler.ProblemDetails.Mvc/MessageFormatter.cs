using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.ProblemDetails.Mvc
{
    public static class MessageFormatters
    {           
        public static void WithProblemDetails<TException>(this IHandledFormatters<TException> formatter, Func<TException, Microsoft.AspNetCore.Mvc.ProblemDetails> details) where TException : Exception
        {
            Task Formatter(TException e, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                c.WriteProblemDetailsAsyncObject(details(e));
                return Task.CompletedTask;
            }

            formatter.WithBody(Formatter);
        }
    }
}