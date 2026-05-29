using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerceAPI.Authorization
{
    public class RightsAuthorizationHandler : AuthorizationHandler<HasRightRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasRightRequirement requirement)
        {
            var rights = context.User.FindAll("right")
                                     .Select(c => c.Value)
                                     .ToList();

            if (rights.Contains(requirement.Right))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}