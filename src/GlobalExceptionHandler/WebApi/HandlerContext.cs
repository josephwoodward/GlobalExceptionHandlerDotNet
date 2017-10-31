using System.Net;

namespace GlobalExceptionHandler.WebApi
{
    public class HandlerContext
    {
        public string DefaultContentType { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}