using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GlobalExceptionHandler.Mvc
{
    public class MvcExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<HandlerConfiguration> _hydrateConfiguration;
        private ConcurrentDictionary<Type, RerouteLocation> _exceptionCodesMapping;

        public MvcExceptionHandlingMiddleware(RequestDelegate next, Action<HandlerConfiguration> hydrateConfiguration)
        {
            _next = next;
            _hydrateConfiguration = hydrateConfiguration;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_exceptionCodesMapping == null)
            {
                var config = new HandlerConfiguration();
                _hydrateConfiguration(config);
                _exceptionCodesMapping = config.BuildOptions();
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                
                RerouteLocation routeLocation;
                var res = _exceptionCodesMapping.TryGetValue(exceptionType, out routeLocation);
                if (res){
                    var res2 = routeLocation;
                    // Redirect
                }
            }
        }
    }
}