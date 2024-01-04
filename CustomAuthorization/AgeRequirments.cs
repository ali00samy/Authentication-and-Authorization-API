using Microsoft.AspNetCore.Authorization;

namespace Authantication.CustomAuthorization
{
    public class AgeRequirments : IAuthorizationRequirement
    {
        public AgeRequirments(int above)
        {
            Above = above;

        }

        public int Above { get; set; }

    }

    public class AgeRequirmentsHandller : AuthorizationHandler<AgeRequirments>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AgeRequirments requirement)
        {
            var ageClaim = context.User.FindFirst(c => c.Type == "Age");
            if (ageClaim == null)
            {
                return Task.CompletedTask;
            }

            var age = int.Parse(ageClaim.Value);

            if (age >= requirement.Above)
            {
                context.Succeed(requirement); 
            }
            return Task.CompletedTask;
        }
    }
}
