using API1.Controllers.General;
using API1.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : MyAppAuthBaseController
    {
        [HttpGet("generate")]
        public IActionResult GenerateReport()
        {
            return Ok(new
            {
                Message = "Report generated successfully!"
            });
        }
    }
}
