using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public CompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
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
            List<Project> result = await _context.Projects
                .Where(project => project.CompanyId == companyId)
                .Include(project => project.Members)
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
    }
}