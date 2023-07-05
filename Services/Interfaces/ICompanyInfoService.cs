using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface ICompanyInfoService
    {
        public Task<Company?> GetCompanyInfoByIdAsync(int? companyId);
        public Task<List<AppUser>> GetAllMembersAsync(int companyId);
        public Task<List<Project>> GetAllProjectsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsAsync(int companyId);
    }
}