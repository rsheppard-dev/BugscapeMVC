using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public CompanyInfoService(
            ApplicationDbContext context,
            IFileService fileService
        )
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<bool> DeleteCompanyLogoAsync(int companyId)
        {
            try
            {
                Company company = _context.Companies.Find(companyId) ?? throw new Exception("Unable to find company.");

                company.LogoFileData = null;
                company.LogoContentType = null;
                company.LogoFileName = null;

                _context.Update(company);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<List<AppUser>> GetAllMembersAsync(int companyId)
        {
            List<AppUser> result = new();

            result = await _context.Users
                .Where(user => user.CompanyId == companyId)
                .ToListAsync();

            return result;
        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            try
            {
                List<Project> result = await _context.Projects
                    .Where(project => project.CompanyId == companyId)
                    .Include(project => project.ProjectPriority)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.Comments)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.Attachments)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.History)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.Notifications)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.DeveloperUser)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.OwnerUser)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.TicketStatus)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.TicketPriority)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.TicketType)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving projects. {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Project> projects = await GetAllProjectsAsync(companyId);
            List<Ticket> result = projects.SelectMany(project => project.Tickets).ToList();

            return result;
        }

        public async Task<Company?> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company? result = await _context.Companies
                    .Include(company => company.Members)
                    .Include(company => company.Projects)
                    .Include(company => company.Invites)
                    .FirstOrDefaultAsync(company => company.Id == companyId);

            return result;
        }

        public async Task<bool> UpdateCompanyLogoAsync(int companyId, IFormFile logo)
        {
            try
            {
                Company company = _context.Companies.Find(companyId) ?? throw new Exception("Unable to find company.");

                company.LogoFileData = await _fileService.ConvertFileToByteArrayAsync(logo);
                company.LogoContentType = logo.ContentType;
                company.LogoFileName = logo.FileName;

                _context.Update(company);
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