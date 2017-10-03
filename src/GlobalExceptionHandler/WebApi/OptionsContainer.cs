using System;
using System.Collections.Concurrent;
using System.Net;

namespace GlobalExceptionHandler.WebApi
{
    public class OptionsContainer
    {
        public ConcurrentDictionary<Type, IExceptionConfig> Exceptions { get; set; }

        public Func<Exception, string> GlobalFormatter { get; set; }
        
        public HttpStatusCode GlobalStatusCode { get; set; }
    }
}