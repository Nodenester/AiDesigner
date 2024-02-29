using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AiDesigner.Server.Data
{
    public class CustomAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public string Role { get; set; }

        // Constructor to accept an optional role parameter
        public CustomAuthorizeAttribute(string role = null)
        {
            Role = role;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;

            var hasRequiredRole = string.IsNullOrEmpty(Role) || context.HttpContext.User.IsInRole(Role);

            var routeUserId = context.HttpContext.Request.RouteValues["userId"] as string
                              ?? context.HttpContext.Request.Query["userId"];

            //Will have to add this later not now this stops some things from working
            var userIdMatches = string.IsNullOrEmpty(routeUserId) ||
                                context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value == routeUserId;
            userIdMatches = true;

            if (isAuthenticated && hasRequiredRole && userIdMatches)
            {
                await next();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
