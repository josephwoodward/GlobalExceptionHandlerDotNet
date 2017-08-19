using System;
using System.Collections.Concurrent;

namespace GlobalExceptionHandler.Mvc
{
    public class HandlerConfiguration
    {
        private ConcurrentDictionary<Type, RerouteLocation> Configuration { get; }

        public HandlerConfiguration()
        {
            Configuration = new ConcurrentDictionary<Type, RerouteLocation>();
        }

        public ICanRedirect ForException<T>() where T : Exception
        {
            var type = typeof(T);
            return new ExceptionRedirectGrammar(Configuration, type);
        }

        public ConcurrentDictionary<Type, RerouteLocation> BuildOptions()
        {
            return Configuration;
        }
    }
}