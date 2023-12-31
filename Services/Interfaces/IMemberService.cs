using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface IMemberService
    {
        public Task<AppUser> GetMemberByIdAsync(int companyId, string userId);
        public Task<bool> RemoveMemberAsync(AppUser member);
    }
}