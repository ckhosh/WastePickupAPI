using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WastePickupAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;
        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        [HttpPost("")]
        public async Task<ActionResult<bool>> AnyTypeOfGetCall([FromBody] Object anyObject)
        {
            _logger.LogInformation("BlankAction Called");
            _logger.LogInformation(anyObject.ToString());
            return Ok(true);
        }

        [HttpPost("{anyAction}")]
        public async Task<ActionResult<bool>> AnyTypeOfGetCalled(string anyAction,[FromBody] Object anyObject)
        {
            _logger.LogInformation("AnyAction Called");
            _logger.LogInformation(anyObject.ToString());
            return Ok(true);
        }
    }
}
