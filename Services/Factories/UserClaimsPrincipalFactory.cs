using System.Security.Claims;
using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BugscapeMVC.Services.Factories
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public UserClaimsPrincipalFactory(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()!));

            return identity;
        }
    }
}