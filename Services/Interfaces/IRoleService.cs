using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BugscapeMVC.Services.Interfaces
{
    public interface IRoleService
    {
        public Task<bool> HasRoleAsync(AppUser user, string roleName);
        public Task<List<IdentityRole>> GetRolesAsync();
        public Task<IEnumerable<string>> GetUserRolesAsync(AppUser user);
        public Task<bool> AddUserToRoleAsync(AppUser user, string roleName);
        public Task<bool> RemoveUserFromRoleAsync(AppUser user, string roleName);
        public Task<bool> RemoveUserFromRolesAsync(AppUser user, IEnumerable<string> roles);
        public Task<List<AppUser>> GetUsersInRoleAsync(string roleName, int companyId);
        public Task<List<AppUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        public Task<string?> GetRoleNameByIdAsync(string roleId);
    }
}