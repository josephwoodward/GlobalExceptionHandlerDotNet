using System;
using System.Net;

namespace GlobalExceptionHandler.Tests.Exceptions
{
    public class HttpNotFoundException : Exception
    {
        public HttpNotFoundException()
        {
        }

        public HttpNotFoundException(string message) : base(message)
        {
        }

        public HttpNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        public HttpStatusCode StatusCodeEnum
            => HttpStatusCode.NotFound;

        public int StatusCodeInt
            => 404;
    }
}