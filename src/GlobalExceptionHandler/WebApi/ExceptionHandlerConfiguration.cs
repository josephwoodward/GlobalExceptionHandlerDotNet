using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GlobalExceptionHandler.ContentNegotiation.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
	public class ExceptionHandlerConfiguration : IUnhandledFormatters<Exception>
	{
		private Type[] _exceptionConfigurationTypesSortedByDepthDescending;
		private Func<Exception, HttpContext, Task> _logger;

		private Func<Exception, HttpContext, HandlerContext, Task> CustomFormatter { get; set; }
		private Func<Exception, HttpContext, HandlerContext, Task> DefaultFormatter { get; }
		private IDictionary<Type, ExceptionConfig> ExceptionConfiguration { get; } = new Dictionary<Type, ExceptionConfig>();

		public string ContentType { get; set; }

		public int DefaultStatusCode { get; set; } = StatusCodes.Status500InternalServerError;
		public bool DebugMode { get; set; }

		public ExceptionHandlerConfiguration(Func<Exception, HttpContext, HandlerContext, Task> defaultFormatter) 
			=> DefaultFormatter = defaultFormatter;

		public IHasStatusCode<TException> Map<TException>() where TException : Exception
			=> new ExceptionRuleCreator<TException>(ExceptionConfiguration);

		public void ResponseBody<T>(Func<Exception, T> formatter) where T : class
		{

            Task Formatter(Exception exception, HttpContext c, HandlerContext b)
            {
                c.Response.ContentType = null;
                var res = formatter(exception);
                c.WriteAsyncObject(res);
                return Task.CompletedTask;
            }

            CustomFormatter = Formatter;
		}

		public void ResponseBody(Func<Exception, string> formatter)
		{
			Task Formatter(Exception x, HttpContext y, HandlerContext b)
			{
				var s = formatter.Invoke(x);
				return y.Response.WriteAsync(s);
			}

			ResponseBody(Formatter);
		}

		public void ResponseBody(Func<Exception, HttpContext, Task> formatter)
		{
			Task Formatter(Exception x, HttpContext y, HandlerContext b)
				=> formatter.Invoke(x, y);

			ResponseBody(Formatter);
		}

		public void ResponseBody(Func<Exception, HttpContext, string> formatter)
		{
			Task Formatter(Exception x, HttpContext y, HandlerContext b)
			{
				var s = formatter.Invoke(x, y);
				return y.Response.WriteAsync(s);
			}

			ResponseBody(Formatter);
		}

		public void ResponseBody(Func<Exception, HttpContext, HandlerContext, Task> formatter)
		{
			CustomFormatter = formatter;
		}

		public void OnError(Func<Exception, HttpContext, Task> log)
			=> _logger = log;

		internal RequestDelegate BuildHandler()
		{
			var handlerContext = new HandlerContext
			{
				ContentType = ContentType
			};

			_exceptionConfigurationTypesSortedByDepthDescending = ExceptionConfiguration.Keys
				.OrderByDescending(x => x, new ExceptionTypePolymorphicComparer())
				.ToArray();

			return async context =>
			{
				var exception = context.Features.Get<IExceptionHandlerFeature>().Error;

				if (ContentType != null)
					context.Response.ContentType = ContentType;

				// If any custom exceptions are set
				foreach (Type type in _exceptionConfigurationTypesSortedByDepthDescending)
				{
					// ReSharper disable once UseMethodIsInstanceOfType TODO: Fire those guys
					if (!type.IsAssignableFrom(exception.GetType()))
						continue;

					var config = ExceptionConfiguration[type];
					context.Response.StatusCode = config.StatusCodeResolver?.Invoke(exception) ?? DefaultStatusCode;

					if (config.Formatter == null)
						config.Formatter = CustomFormatter;

                    if (_logger != null)
                        await _logger(exception, context);

					await config.Formatter(exception, context, handlerContext);
					return;
				}

				// Global default format output
				if (CustomFormatter != null)
				{
					context.Response.StatusCode = DefaultStatusCode;
					await CustomFormatter(exception, context, handlerContext);
					return;
				}

				if (DebugMode)
					await DefaultFormatter(exception, context, handlerContext);
			};
		}
	}
}