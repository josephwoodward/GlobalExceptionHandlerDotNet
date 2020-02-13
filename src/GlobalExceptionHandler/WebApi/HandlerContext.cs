using System;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public class HandlerContext
    {
        public string ContentType { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }

    public class ExceptionContext
    {
        public Exception Exception { get; set; }

        public Type ExceptionMatched { get; set; }

        public HttpContext HttpContext { get; set; }
    }
}