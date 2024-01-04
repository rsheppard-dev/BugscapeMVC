using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;


namespace BugscapeMVC.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileService _fileService;


        public AccountService(UserManager<AppUser> userManager, IFileService fileService)
        {
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<bool> DeleteUserImageAsync(AppUser user)
        {
            try
            {
                user.AvatarFileData = null;
                user.AvatarContentType = null;
                user.AvatarFileName = null;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    throw new Exception("Unable to delete user image.");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> UpdateUserImageAsync(AppUser user, IFormFile imageFormFile)
        {
            try
            {
                user.AvatarFileData = await _fileService.ConvertFileToByteArrayAsync(imageFormFile);
                user.AvatarContentType = imageFormFile.ContentType;
                user.AvatarFileName = imageFormFile.FileName;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    throw new Exception("Unable to update user image.");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
    }
}