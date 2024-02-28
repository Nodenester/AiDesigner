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

            // Check if a role is specified and if the user has that role
            var hasRequiredRole = string.IsNullOrEmpty(Role) || context.HttpContext.User.IsInRole(Role);

            // Extract userId from the route data or query string
            var routeUserId = context.HttpContext.Request.RouteValues["userId"] as string
                              ?? context.HttpContext.Request.Query["userId"];

            // Check if userId is provided and matches the current user's Id
            var userIdMatches = string.IsNullOrEmpty(routeUserId) ||
                                context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value == routeUserId;

            if (isAuthenticated && hasRequiredRole && userIdMatches)
            {
                // User is authenticated, has the required role (if specified), and userId matches (if provided), proceed with the request
                await next();
            }
            else
            {
                // User is not authenticated, doesn't have the required role, or userId doesn't match, block the request
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
