using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GlobalExceptionHandler.WebApi
{
	public class ExceptionHandlerConfiguration : IUnhandledFormatters
	{
		private readonly IDictionary<Type, ExceptionConfig> _exceptionConfiguration = new Dictionary<Type, ExceptionConfig>();
		private Type[] _exceptionConfigurationTypesSortedByDepthDescending;
		private Func<Exception, HttpContext, Task> _logger;

		internal Func<Exception, HttpContext, HandlerContext, Task> CustomFormatter { get; private set; }
		internal Func<Exception, HttpContext, HandlerContext, Task> DefaultFormatter { get; }
		internal IDictionary<Type, ExceptionConfig> ExceptionConfiguration => _exceptionConfiguration;

		public string ContentType { get; set; }
		public bool DebugMode { get; set; }

		public ExceptionHandlerConfiguration(Func<Exception, HttpContext, HandlerContext, Task> defaultFormatter) => DefaultFormatter = defaultFormatter;

		[Obsolete("ForException<T> is obsolete and will be removed soon, use Map<T> instead", false)]
		public IHasStatusCode ForException<T>() where T : Exception
		{
			var type = typeof(T);
			return new ExceptionRuleCreator(_exceptionConfiguration, type);
		}

		public IHasStatusCode<TException> Map<TException>() where TException : Exception
			=> new ExceptionRuleCreator<TException>(_exceptionConfiguration);

		[Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use ResponseBody(..) instead", false)]
		public void MessageFormatter(Func<Exception, string> formatter)
			=> ResponseBody(formatter);

		[Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use ResponseBody(..) instead", false)]
		public void MessageFormatter(Func<Exception, HttpContext, string> formatter)
			=> ResponseBody(formatter);

		[Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use ResponseBody(..) instead", false)]
		public void MessageFormatter(Func<Exception, HttpContext, Task> formatter)
			=> ResponseBody(formatter);

		[Obsolete("MessageFormatter(..) is obsolete and will be removed soon, use ResponseBody(..) instead", false)]
		public void MessageFormatter(Func<Exception, HttpContext, HandlerContext, Task> formatter)
			=> ResponseBody(formatter);

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
			{
				return formatter.Invoke(x, y);
			}

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
		{
			_logger = log;
		}

		internal RequestDelegate BuildHandler()
		{
			var handlerContext = new HandlerContext
			{
				ContentType = ContentType
			};

			_exceptionConfigurationTypesSortedByDepthDescending = _exceptionConfiguration.Keys
				.OrderByDescending(x => x, new ExceptionTypePolymorphicComparer())
				.ToArray();

			return async context =>
			{
				var s = context.RequestServices.GetService(typeof(IServiceProvider)) as IServiceProvider;
				var logger = s.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
				
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
					context.Response.StatusCode = config.StatusCodeResolver(exception);

					if (config.Formatter == null)
						config.Formatter = CustomFormatter;
					
					// TODO: Exception not handled? We want the consumer to know about it.
					// TODO: 1: Let the .NET middleware raise the exception
                    if (_logger != null)
                        await _logger(exception, context);

					await config.Formatter(exception, context, handlerContext);
					return;
				}

				// Global default format output
				if (CustomFormatter != null)
				{
					await CustomFormatter(exception, context, handlerContext);
					return;
				}

				if (DebugMode)
					await DefaultFormatter(exception, context, handlerContext);
			};
		}
	}
}