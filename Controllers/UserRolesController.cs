using BugscapeMVC.Extensions;
using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ICompanyInfoService _companyInfoService;

        public UserRolesController(IRoleService roleService, ICompanyInfoService companyInfoService)
        {
            _roleService = roleService;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = User.Identity?.GetCompanyId() ?? 0;

            if (companyId == 0) return View(model);

            List<AppUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (AppUser user in users)
            {
                IEnumerable<string> selected = await _roleService.GetUserRolesAsync(user);

                ManageUserRolesViewModel viewModel = new()
                {
                    AppUser = user,
                    Roles = new MultiSelectList(await _roleService.GetRolesAsync(), "Name", "Name", selected),
                };

                model.Add(viewModel);
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity?.GetCompanyId() ?? 0;

            if (companyId == 0) return View(member);

            AppUser? user = (await _companyInfoService.GetAllMembersAsync(companyId))
                .FirstOrDefault(user => user.Id == member.AppUser.Id);

            if (user is null) return View(member);

            IEnumerable<string> roles = await _roleService.GetUserRolesAsync(user);

            string? selectedUserRole = member.SelectedRoles?.FirstOrDefault();

            if (member.SelectedRoles is not null && member.SelectedRoles.Any())
            {
                await _roleService.RemoveUserFromRolesAsync(user, roles);

                foreach (string selectedRole in member.SelectedRoles)
                {
                    await _roleService.AddUserToRoleAsync(user, selectedRole);
                }
            }

            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}