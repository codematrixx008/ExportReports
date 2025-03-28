using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Diagnostics;
using API1.Service;
using Microsoft.AspNetCore.Authorization;
using API1.Interface;

namespace API1.Filters
{
    public class RoleAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly IUserService _userService;

        public RoleAuthorizationFilter(IUserService userService)
        {
            _userService = userService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute);
            if (hasAllowAnonymous)
            {
                return;
            }

            bool isAuthorized = await CheckUserAuthorizationAsync(context);
            if (!isAuthorized)
            {
                if (context.Result == null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Home" },   // Change "Account" to your login controller name
                    { "action", "Login" }       // Change "Login" to your login action method name
                });
                }
            }
        }

        private async Task<bool> CheckUserAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                var userRoles = user.Claims
                                    .Where(c => c.Type == ClaimTypes.Role)
                                    .Select(c => c.Value)
                                    .ToList();

                var controllerName = context.RouteData.Values["controller"]?.ToString();
                var actionName = context.RouteData.Values["action"]?.ToString();

                if (await _userService.HasControllerAccessAsync(userRoles, controllerName, actionName))
                {
                    return true;
                }
                else
                {
                    context.Result = new ForbidResult();
                }
            }
            return false;
        }
    }
}


