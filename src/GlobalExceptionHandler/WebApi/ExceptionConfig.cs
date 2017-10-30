using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
	public class ExceptionConfig
	{
		public HttpStatusCode StatusCode { get; set; }
		public Func<ExceptionHandlerContext, Task> Formatter { get; set; } = DefaultFormatter;

		public static Task DefaultFormatter(Exception exception, HttpContext httpContext)
			=> httpContext.Response.WriteAsync(exception.ToString());

		public static Task SimpleMessageWithNoDetails(Exception exception, HttpContext httpContext)
			=> httpContext.Response.WriteAsync("An error occurred while processing your request");
	}
}