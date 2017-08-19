using System.Collections.Generic;
using GlobalExceptionHandler.Tests.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.Tests.WebApi
{
    [Route("api/boo")]
    public class TestController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            throw new ProductNotFoundException();
        }
    }
}