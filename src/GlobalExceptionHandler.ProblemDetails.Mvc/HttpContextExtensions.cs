using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.ProblemDetails.Mvc
{
    public static class HttpContextExtensions
    {
        public static Task WriteProblemDetailsAsyncObject(this HttpContext context, Microsoft.AspNetCore.Mvc.ProblemDetails details)
        {
            var requestResult = new ObjectResult(details)
            {
                ContentTypes = {"application/problem+json", "application/problem+xml"},
                StatusCode = 500
            };
            
            var emptyActionContext = new EmptyActionContext(context);
            /*var result = new ObjectResult(requestResult);*/
            return requestResult.ExecuteResultAsync(emptyActionContext);
        }
    }
}