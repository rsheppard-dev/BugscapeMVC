using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface ICompanyInfoService
    {
        public Task<bool> DeleteCompanyLogoAsync(int companyId);
        public Task<Company?> GetCompanyInfoByIdAsync(int? companyId);
        public Task<List<AppUser>> GetAllMembersAsync(int companyId);
        public Task<List<Project>> GetAllProjectsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsAsync(int companyId);
        public Task<bool> UpdateCompanyLogoAsync(int companyId, IFormFile logo);
    }
}