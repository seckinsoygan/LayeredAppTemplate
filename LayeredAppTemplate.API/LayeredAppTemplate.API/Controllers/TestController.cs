using Microsoft.AspNetCore.Mvc;

namespace LayeredAppTemplate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("throw")]
        public IActionResult ThrowException()
        {
            throw new Exception("Test exception");
        }
    }
}
