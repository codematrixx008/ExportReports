using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API1.Service;
using API1.Interface;

namespace API1.Filters
{
    public class RoleActionFilter : IAsyncActionFilter
    {
        private readonly IUserService _userService;

        public RoleActionFilter(IUserService userService)
        {
            _userService = userService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRoles = user.Claims
                                .Where(c => c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList();

            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            if (await _userService.HasControllerAccessAsync(userRoles, controllerName, actionName))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();  // Continue execution
        }


    }
}
