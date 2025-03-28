using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers
{
    [Route("api/securepage")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminProtectedData()
        {
            return Ok(new { Message = "You have accessed a protected route!" });
        }

        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public IActionResult GetUserProtectedData()
        {
            return Ok(new { Message = "You have accessed a protected route!" });
        }
    }
}
