using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.ContentNegotiation
{
    public static class HttpContextExtensions
    {
        public static Task WriteAsyncObject(this HttpContext context, object value)
        {
            // Using content negotiation API so empty content type for MVC pipeline to infer type
            context.Response.ContentType = null;

            var emptyActionContext = new EmptyActionContext(context);
            var result = new ObjectResult(value);

            return result.ExecuteResultAsync(emptyActionContext);
        }
    }
}