using BugscapeMVC.Extensions;
using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class UserRolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ICompanyInfoService _companyInfoService;
        private readonly UserManager<AppUser> _userManager;

        public UserRolesController(
            IRoleService roleService,
            ICompanyInfoService companyInfoService,
            UserManager<AppUser> userManager
        )
        {
            _roleService = roleService;
            _companyInfoService = companyInfoService;
            _userManager = userManager;
        }

        public async Task<IActionResult> ManageUserRoles(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = User.Identity?.GetCompanyId() ?? 0;
            string? userId = _userManager.GetUserId(User);

            if (companyId == 0 || string.IsNullOrEmpty(userId)) return View(model);

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<AppUser> members = await _companyInfoService.GetAllMembersAsync(companyId);
            List<IdentityRole> roles = await _roleService.GetRolesAsync();

            members = members.Where(m => m.Id != userId).ToList();

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                members = Search(members, search);
            }

            members = await Sort(_userManager, members, sortBy, order);

            IEnumerable<SelectListItem> roleSelectList = roles
                .Where(r => r.Name != nameof(Roles.Demo_User))
                .Select(r => new SelectListItem 
                { 
                    Value = r.Name, 
                    Text = r.Name?.Replace("_", " ") 
                });

            foreach (AppUser member in members)
            {
                IEnumerable<string> selected = await _roleService.GetUserRolesAsync(member);

                ManageUserRolesViewModel viewModel = new()
                {
                    AppUser = member,
                    Roles = new SelectList(roleSelectList, "Value", "Text", selected.FirstOrDefault()),
                };

                model.Add(viewModel);
            }
            
            return View(new PaginatedList<ManageUserRolesViewModel>(model, page, limit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member, int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            int companyId = User.Identity?.GetCompanyId() ?? 0;

            if (companyId == 0) return View(member);

            AppUser? user = (await _companyInfoService.GetAllMembersAsync(companyId))
                .FirstOrDefault(user => user.Id == member.AppUser.Id);

            if (user is null) return View(member);

            IEnumerable<string> roles = await _roleService.GetUserRolesAsync(user);

            string? selectedUserRole = member.SelectedRole;

            if (member.SelectedRole is not null && member.SelectedRole.Any())
            {
                await _roleService.RemoveUserFromRolesAsync(user, roles);

                await _roleService.AddUserToRoleAsync(user, member.SelectedRole);
            }

            return RedirectToAction(nameof(ManageUserRoles), new { page, search, order, sortBy, limit });
        }

        private static async Task<List<AppUser>> Sort(UserManager<AppUser> userManager, List<AppUser> members, string sortBy = "name", string order = "asc")
        {
            // Load the members and their roles into memory
            var memberRoles = new List<(AppUser Member, IList<string> Roles)>();
            foreach (var member in members)
            {
                var roles = await userManager.GetRolesAsync(member);
                memberRoles.Add((member, roles));
            }

            // Sort the members
            var sortedMembers = sortBy.ToLower() switch
            {
                "role" => order == "asc" ?
                    memberRoles.OrderBy(m => m.Roles.FirstOrDefault()).ToList() :
                    memberRoles.OrderByDescending(m => m.Roles.FirstOrDefault()).ToList(),
                _ => order == "asc" ?
                    memberRoles.OrderBy(m => m.Member.FullName).ToList() :
                    memberRoles.OrderByDescending(m => m.Member.FullName).ToList(),
            };

            // Return the sorted members
            return sortedMembers.Select(m => m.Member).ToList();
        }

        private static List<AppUser> Search(List<AppUser> members, string search)
        {
            if (members is null) return new List<AppUser>();
            
            return members
                .Where(m => m.FullName?.ToLower().Contains(search.ToLower()) ?? false)
                .ToList();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}