using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandlerDotNet.Mvc
{
    public static class HttpContextExtensions
    {
	    public static Task WriteObjectAsync(this HttpContext context, object value)
	    {
			var nullActionContext = new NullActionContext(context);
		    var result = new ObjectResult(value);
		    return result.ExecuteResultAsync(nullActionContext);
		}
    }
}
