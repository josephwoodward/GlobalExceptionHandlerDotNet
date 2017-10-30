using System;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public class ExceptionHandlerContext
    {
        public string DefaultContentType { get; set; }

        public Exception Exception { get; set; }

        public HttpContext HttpContext { get; set; }
    }
}