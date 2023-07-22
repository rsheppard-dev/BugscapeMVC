using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;
        private readonly IProjectService _projectService;
        
        public TicketService(ApplicationDbContext context, IRoleService roleService, IProjectService projectService)
        {
            _context = context;
            _roleService = roleService;
            _projectService = projectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
           try
           {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
           }
           catch (Exception)
           {
            throw;
           }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket? ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket is null) return;

            try
            {
                ticket.DeveloperUserId = userId;
                // revisit code when assigning tickets - update to enum
                ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                    .Where(project => project.CompanyId == companyId)
                    .SelectMany(project => project.Tickets)
                        .Include(ticket => ticket.Attachments)
                        .Include(ticket => ticket.Comments)
                        .Include(ticket => ticket.DeveloperUser)
                        .Include(ticket => ticket.History)
                        .Include(ticket => ticket.Notifications)
                        .Include(ticket => ticket.OwnerUser)
                        .Include(ticket => ticket.TicketPriority)
                        .Include(ticket => ticket.TicketStatus)
                        .Include(ticket => ticket.TicketType)
                        .Include(ticket => ticket.Project)
                    .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

            try 
            {
                List<Ticket> tickets = await _context.Projects
                    .Where(project => project.CompanyId == companyId)
                    .SelectMany(project => project.Tickets)
                        .Include(ticket => ticket.Attachments)
                        .Include(ticket => ticket.Comments)
                        .Include(ticket => ticket.DeveloperUser)
                        .Include(ticket => ticket.History)
                        .Include(ticket => ticket.Notifications)
                        .Include(ticket => ticket.OwnerUser)
                        .Include(ticket => ticket.TicketPriority)
                        .Include(ticket => ticket.TicketStatus)
                        .Include(ticket => ticket.TicketType)
                        .Include(ticket => ticket.Project)
                    .Where(ticket => ticket.TicketPriorityId == priorityId)
                    .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects
                    .Where(project => project.CompanyId == companyId)
                    .SelectMany(project => project.Tickets)
                        .Include(ticket => ticket.Attachments)
                        .Include(ticket => ticket.Comments)
                        .Include(ticket => ticket.DeveloperUser)
                        .Include(ticket => ticket.History)
                        .Include(ticket => ticket.Notifications)
                        .Include(ticket => ticket.OwnerUser)
                        .Include(ticket => ticket.TicketPriority)
                        .Include(ticket => ticket.TicketStatus)
                        .Include(ticket => ticket.TicketType)
                        .Include(ticket => ticket.Project)
                    .Where(ticket => ticket.TicketStatusId == statusId)
                    .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects
                    .Where(project => project.CompanyId == companyId)
                    .SelectMany(project => project.Tickets)
                        .Include(ticket => ticket.Attachments)
                        .Include(ticket => ticket.Comments)
                        .Include(ticket => ticket.DeveloperUser)
                        .Include(ticket => ticket.History)
                        .Include(ticket => ticket.Notifications)
                        .Include(ticket => ticket.OwnerUser)
                        .Include(ticket => ticket.TicketPriority)
                        .Include(ticket => ticket.TicketStatus)
                        .Include(ticket => ticket.TicketType)
                        .Include(ticket => ticket.Project)
                    .Where(ticket => ticket.TicketTypeId == typeId)
                    .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId))
                .Where(ticket => ticket.Archived)
                .ToList();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName))
                    .Where(ticket => ticket.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetTicketsByRoleAsync(role, userId, companyId))
                    .Where(ticket => ticket.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByStatusAsync(companyId, statusName))
                    .Where(ticket => ticket.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByTypeAsync(companyId, typeName))
                    .Where(ticket => ticket.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByUserIdAsync(string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByCompanyAsync(companyId))
                    .Where(ticket => ticket.OwnerUserId == userId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets.FindAsync(ticketId);
            }
            catch (Exception)
            {  
                throw;
            }
        }

        public async Task<AppUser?> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            AppUser? developer = new();

            try
            {
                Ticket? tickets = (await GetAllTicketsByCompanyAsync(companyId))
                    .FirstOrDefault(ticket => ticket.Id == ticketId);

                if (tickets is null || tickets.DeveloperUser is null) return null;

                developer = tickets.DeveloperUser;

                return developer;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                if (Enum.TryParse(role, out Roles parsedRole))
                {
                    switch (parsedRole)
                    {
                        case Roles.Admin:
                            tickets = await GetAllTicketsByCompanyAsync(companyId);
                            break;
                        case Roles.ProjectManager:
                            tickets = await GetTicketsByUserIdAsync(userId, companyId);
                            break;
                        case Roles.Developer:
                            tickets = (await GetAllTicketsByCompanyAsync(companyId))
                                .Where(ticket => ticket.DeveloperUserId == userId)
                                .ToList();
                            break;
                        case Roles.Submitter:
                            tickets = (await GetAllTicketsByCompanyAsync(companyId))
                                .Where(ticket => ticket.OwnerUserId == userId)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            List<Ticket> tickets = new();

            AppUser? user = await _context.Users.FindAsync(userId);

            if (user is null) return tickets;

            try
            {
                if (await _roleService.HasRoleAsync(user, Roles.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                        .SelectMany(project => project.Tickets)
                        .ToList();
                }
                else if (await _roleService.HasRoleAsync(user, Roles.Developer.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                        .SelectMany(project => project.Tickets)
                        .Where(ticket => ticket.DeveloperUserId == userId)
                        .ToList();
                }
                else if (await _roleService.HasRoleAsync(user, Roles.Submitter.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                        .SelectMany(project => project.Tickets)
                        .Where(ticket => ticket.OwnerUserId == userId)
                        .ToList();
                }
                else if (await _roleService.HasRoleAsync(user, Roles.ProjectManager.ToString()))
                {
                    tickets = (await _projectService.GetUserProjectsAsync(userId))
                        .SelectMany(project => project.Tickets)
                        .ToList();
                }

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                return (await _context.TicketPriorities
                .FirstOrDefaultAsync(priority => priority.Name == priorityName))?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                return (await _context.TicketStatuses
                .FirstOrDefaultAsync(status => status.Name == statusName))?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                return (await _context.TicketTypes
                .FirstOrDefaultAsync(type => type.Name == typeName))?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {   
                throw;
            }
        }
    }
}