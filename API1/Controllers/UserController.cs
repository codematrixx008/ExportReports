using API1.Controllers.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : MyAppAuthBaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                Message = "User Dashboard Accessed!"
            });
        }
    }
}
