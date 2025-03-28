using API1.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API1.Controllers.General
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(RoleAuthorizationFilter))] // Use ServiceFilter for DI
    public class MyAppAuthBaseController : ControllerBase
    {
        protected string? GetUserId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        protected List<string> GetUserRoles()
        {
            return User.Claims
                       .Where(c => c.Type == ClaimTypes.Role)
                       .Select(c => c.Value)
                       .ToList();
        }
    }
}
