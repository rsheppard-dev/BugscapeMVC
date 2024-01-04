using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugscapeMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            IAccountService accountService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration
        )
        {
            _accountService = accountService;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> DemoLogin(string role)
        {
            // define demo users for each role
            var demoUsers = new Dictionary<string, string>
            {
                {"admin", "demoadmin@bugscape.com"},
                {"project_manager", "demopm@bugscape.com"},
                {"developer", "demodev@bugscape.com"},
                {"submitter", "demosub@bugscape.com"}
            };

            if (!demoUsers.TryGetValue(role, out string? value))
            {
                return BadRequest("Invalid role");
            }

            var username = value;
            var password = _configuration[$"Demo:Password"];

            var user = await _userManager.FindByNameAsync(username);
            
            if (user is null || string.IsNullOrEmpty(password)) return BadRequest("Invalid username or password");

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            return BadRequest("Invalid password");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserImage([Bind("AvatarFormFile")] AppUser model)
        {
            try
            {
                AppUser user = await _userManager.GetUserAsync(User) ?? throw new Exception("Unable to find user.");
                IFormFile imageFormFile = model.AvatarFormFile ?? throw new Exception("Unable to find image.");

                bool result = await _accountService.UpdateUserImageAsync(user, imageFormFile);

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

        [Authorize]
        public async Task<IActionResult> DeleteUserImage()
        {
            try
            {
                AppUser user = await _userManager.GetUserAsync(User) ?? throw new Exception("Unable to find user.");

                bool result = await _accountService.DeleteUserImageAsync(user);

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