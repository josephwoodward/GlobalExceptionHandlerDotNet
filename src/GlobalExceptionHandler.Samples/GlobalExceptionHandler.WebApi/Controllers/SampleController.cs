using System;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.WebApi.Controllers
{
    [Route("api/sample")]
    public class Sampleontroller : Controller
    {
        [HttpGet("{id}")]
        public IActionResult Get(int id) => throw new ArgumentException($"Event {id} could not be found");
    }
}