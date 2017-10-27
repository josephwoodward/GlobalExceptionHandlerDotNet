using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.WebApi
{
	public class ExceptionHandlerConfiguration
	{
		public ExceptionHandlerConfiguration(Func<Exception, HttpContext, Task> defaultFormatter) => DefaultFormatter = defaultFormatter;

		public IHasStatusCode ForException<T>() where T : Exception
		{
			var type = typeof(T);
			return new ExceptionRuleCreator(_exceptionConfiguration, type);
		}

		public void DefaultMessageFormatter(Func<Exception, HttpContext, Task> formatter)
		{
			DefaultFormatter = formatter;
		}
		
		private readonly IDictionary<Type, ExceptionConfig> _exceptionConfiguration = new Dictionary<Type, ExceptionConfig>();

		private Type[] _exceptionConfgurationTypesSortedByDepthDescending;
		
		private Func<Exception, HttpContext, Task> _logger;

		internal IDictionary<Type, ExceptionConfig> ExceptionConfiguration => _exceptionConfiguration;

		internal Func<Exception, HttpContext, Task> DefaultFormatter { get; private set; }

		public void OnError(Func<Exception, HttpContext, Task> log)
		{
			_logger = log;
		}
		
		internal RequestDelegate BuildHandler()
		{
			_exceptionConfgurationTypesSortedByDepthDescending = _exceptionConfiguration.Keys
				.OrderBy(x => x, new ExceptionTypePolymorphicComparer())
				.ToArray();

			return async context =>
			{
				var exception = context.Features.Get<IExceptionHandlerFeature>().Error;

				foreach (var type in _exceptionConfgurationTypesSortedByDepthDescending)
				{
					// ReSharper disable once UseMethodIsInstanceOfType TODO: Fire those guys
					if (type.IsAssignableFrom(exception.GetType()))
					{
						var config = ExceptionConfiguration[type];
						context.Response.StatusCode = (int)config.StatusCode;
					
						if (_logger != null)
						{
							await _logger(exception, context);
						}
						
						await config.Formatter(exception, context);
						return;
					}
				}
			};
		}
	}
}