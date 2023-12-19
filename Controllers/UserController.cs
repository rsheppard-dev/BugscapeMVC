using BugscapeMVC.Models;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public UserController(IUserService userService, UserManager<AppUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserImage([Bind("AvatarFormFile")] AppUser model)
        {
            try
            {
                AppUser user = await _userManager.GetUserAsync(User) ?? throw new Exception("Unable to find user.");
                IFormFile imageFormFile = model.AvatarFormFile ?? throw new Exception("Unable to find image.");

                bool result = await _userService.UpdateUserImageAsync(user, imageFormFile);

                if (Request.Headers.ContainsKey("Referer"))
                {
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                else
                {
                    return Redirect("/Identity/Account/Manage");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> DeleteUserImage()
        {
            try
            {
                AppUser user = await _userManager.GetUserAsync(User) ?? throw new Exception("Unable to find user.");

                bool result = await _userService.DeleteUserImageAsync(user);

                if (Request.Headers.ContainsKey("Referer"))
                {
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                else
                {
                    return Redirect("/Identity/Account/Manage");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}