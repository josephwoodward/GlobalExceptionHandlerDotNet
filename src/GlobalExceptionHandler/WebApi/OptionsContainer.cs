using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
    public class OptionsContainer
    {
        public ConcurrentDictionary<Type, IExceptionConfig> Exceptions { get; set; }

        public Func<Exception, string> GlobalFormatter { get; set; }
        
        public HttpStatusCode GlobalStatusCode { get; set; }
        
        public Func<HttpContext, Exception, Task> Logger { get; set; }
    }
}