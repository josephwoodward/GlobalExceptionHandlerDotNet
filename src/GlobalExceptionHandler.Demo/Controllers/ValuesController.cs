using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.Demo.Controllers
{
    [Route("api/demo")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            throw new ArgumentException();
            return new[] {"value1", "value2"};
        }
    }
}