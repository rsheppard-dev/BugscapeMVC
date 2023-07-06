using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugscapeMVC.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        public RoleService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<bool> AddUserToRoleAsync(AppUser user, string roleName)
        {
            bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;

            return result;
        }

        public async Task<string?> GetRoleNameByIdAsync(string roleId)
        {
            IdentityRole? role = _context.Roles.Find(roleId);

            if (role is null) return null;       

            string? result = await _roleManager.GetRoleNameAsync(role);

            return result;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(AppUser user)
        {
            IEnumerable<string> result = await _userManager.GetRolesAsync(user);

            return result;
        }

        public async Task<List<AppUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            List<AppUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
            List<AppUser> result = users.Where(users => users.CompanyId == companyId).ToList();

            return result;
        }

        public async Task<List<AppUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(user => user.Id).ToList();
            List<AppUser> roleUsers = _context.Users.Where(user => !userIds.Contains(user.Id)).ToList();
            List<AppUser> result = roleUsers.Where(user => user.CompanyId == companyId).ToList();
            
            return result;
        }

        public async Task<bool> HasRoleAsync(AppUser user, string roleName)
        {
            bool result = await _userManager.IsInRoleAsync(user, roleName);

            return result;
        }

        public async Task<bool> RemoveUserFromRoleAsync(AppUser user, string roleName)
        {
            bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;

            return result;
        }

        public async Task<bool> RemoveUserFromRolesAsync(AppUser user, IEnumerable<string> roles)
        {
            bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;

            return result;
        }
    }
}