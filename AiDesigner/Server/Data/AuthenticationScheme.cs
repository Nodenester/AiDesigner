using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AiDesigner.Server.Data
{
    public class UserNameRequirement : IAuthorizationRequirement
    {
        // Requirement properties can be added here, if needed.
    }

    public class UserNameRequirementHandler : AuthorizationHandler<UserNameRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserNameRequirement requirement)
        {
            var user = context.User;
            if (!string.IsNullOrEmpty(user?.Identity?.Name))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
