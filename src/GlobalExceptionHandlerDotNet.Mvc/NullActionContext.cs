using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace GlobalExceptionHandlerDotNet.Mvc
{
    internal class NullActionContext : ActionContext
    {
        public NullActionContext(HttpContext httpContext) : base(httpContext, new RouteData(), new ActionDescriptor())
        {
        }
    }
}