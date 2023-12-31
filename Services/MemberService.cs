using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class MemberService : IMemberService
    {
        private readonly ApplicationDbContext _context;

        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser> GetMemberByIdAsync(int companyId, string userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(user => user.CompanyId == companyId)
                    .FirstOrDefaultAsync(user => user.Id == userId);

                return user ?? throw new Exception("User not found.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberAsync(AppUser member)
        {
            try
            {
                _context.Users.Remove(member);
                await _context.SaveChangesAsync();

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