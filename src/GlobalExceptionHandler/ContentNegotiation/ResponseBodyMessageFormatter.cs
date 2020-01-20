using System;
using System.Threading.Tasks;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Http;

namespace GlobalExceptionHandler.ContentNegotiation
{
    public class ResponseBodyMessageFormatter
    {
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
/*
            CustomFormatter = formatter;
*/
        }
    }
}