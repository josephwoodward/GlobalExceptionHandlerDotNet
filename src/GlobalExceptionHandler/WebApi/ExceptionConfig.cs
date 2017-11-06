using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
	public class ExceptionConfig
	{
		public HttpStatusCode StatusCode { get; set; }
		
		public Func<Exception, HttpContext, HandlerContext, Task> Formatter { get; set; } = UnsafeFormatterWithDetails;

		public static Task UnsafeFormatterWithDetails(Exception exception, HttpContext httpContext, HandlerContext handlerContext)
			=> httpContext.Response.WriteAsync(exception.ToString());

		public static Task SafeFormatterWithDetails(Exception exception, HttpContext httpContext, HandlerContext handlerContext)
			=> httpContext.Response.WriteAsync("An error occurred while processing your request");
	}
}