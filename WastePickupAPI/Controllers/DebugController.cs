using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WastePickupAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        [HttpPost("")]
        public async Task<ActionResult<bool>> AnyTypeOfGetCall([FromBody] Object anyObject)
        {
            Console.WriteLine("BlankAction Called");
            Console.WriteLine(anyObject.ToString());
            return Ok(true);
        }

        [HttpPost("{anyAction}")]
        public async Task<ActionResult<bool>> AnyTypeOfGetCalled(string anyAction,[FromBody] Object anyObject)
        {
            Console.WriteLine("AnyAction Called");
            Console.WriteLine(anyObject.ToString());
            return Ok(true);
        }
    }
}
