using Microsoft.AspNetCore.Authorization;

namespace ECommerceAPI.Authorization
{
    public class HasRightRequirement : IAuthorizationRequirement
    {
        public string Right { get; }
        public HasRightRequirement(string right) => Right = right;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class HasRightAttribute : AuthorizeAttribute
    {
        public HasRightAttribute(string right) : base(policy: $"Right:{right}") { }
    }
}