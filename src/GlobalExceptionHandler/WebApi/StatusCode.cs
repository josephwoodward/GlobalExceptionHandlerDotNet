using System;
using System.Collections.Generic;
using System.Net;

namespace GlobalExceptionHandler.WebApi
{
    public interface IHasStatusCode
    {
        void ReturnStatusCode(HttpStatusCode statusCode);
    }

    public class StatusCode : IHasStatusCode
    {
        private readonly IDictionary<Type, HttpStatusCode> _configuration;
        private readonly Type _type;

        public StatusCode(IDictionary<Type, HttpStatusCode> configuration, Type type)
        {
            _configuration = configuration;
            _type = type;
        }

        public void ReturnStatusCode(HttpStatusCode statusCode)
        {
            _configuration.Add(_type, statusCode);
        }
    }
}