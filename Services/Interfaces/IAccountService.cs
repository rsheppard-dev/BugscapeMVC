using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<bool> DeleteUserImageAsync(AppUser user);
        public Task<bool> UpdateUserImageAsync(AppUser user, IFormFile imageFormFile);
    }
}