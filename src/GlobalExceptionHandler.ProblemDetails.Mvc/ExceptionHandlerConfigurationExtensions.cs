using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.ProblemDetails.Mvc
{
    public static class ExceptionHandlerConfigurationExtensions
    {
        /*public static void EnableProblemDetails(this ExceptionHandlerConfiguration config, Func<Exception, Microsoft.AspNetCore.Mvc.ProblemDetails> details)
        {
            var c = (ProblemDetailsExt) config;
            c.ProblemDetails = details;
        }*/
    }

    internal class ProblemDetailsExt : ExceptionHandlerConfiguration
    {
        public Func<Exception, Microsoft.AspNetCore.Mvc.ProblemDetails> ProblemDetails { get; set; }
        
        public ProblemDetailsExt(Func<Exception, HttpContext, HandlerContext, Task> defaultFormatter) : base(defaultFormatter)
        {   
        }
        
    }
}