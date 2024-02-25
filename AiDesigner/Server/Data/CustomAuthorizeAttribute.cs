using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

            if (isAuthenticated && hasRequiredRole)
            {
                // User is authenticated and has the required role (if specified), proceed with the request
                await next();
            }
            else
            {
                // User is not authenticated or doesn't have the required role, block the request
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
