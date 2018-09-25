using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.ProblemDetails.Controllers
{
    public class MyRequest
    {
        [Required]
        public string Name { get; set; }
    }
    
    [Route("api/[controller]")]
    /*[ApiController]*/
    public class ValuesController : ControllerBase
    {
        public ValuesController()
        {
            var res = true;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(MyRequest request)
        {
            var msg = request.Name;
            throw new ArgumentException("Your params suck!" + msg);
            /*return ValidationProblem();*/
            /*return BadRequest();*/
            return new string[] {"value1", "value2"};
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}