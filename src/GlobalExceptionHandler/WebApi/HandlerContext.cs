using System.Net;

namespace GlobalExceptionHandler.WebApi
{
    public class HandlerContext
    {
        public string ContentType { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}