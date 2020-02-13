using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandler.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
            => throw new RecordNotFoundException();
    }
}