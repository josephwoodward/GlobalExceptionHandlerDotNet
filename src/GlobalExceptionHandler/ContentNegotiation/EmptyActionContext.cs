using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace GlobalExceptionHandler.ContentNegotiation
{
    internal class EmptyActionContext : ActionContext
    {
        public EmptyActionContext(HttpContext httpContext) : base(httpContext, new RouteData(), new ActionDescriptor())
        {
        }
    }
}