using System;
using System.Net;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.WebApi
{
    public interface IExceptionConfig
    {
        HttpStatusCode ?StatusCode { get; set; }
        Func<Exception, string> Formatter { get; set; }
    }

    public class ExceptionConfig : IExceptionConfig
    {
        public HttpStatusCode ?StatusCode { get; set; }
        public Func<Exception, string> Formatter { get; set; }
    }
}