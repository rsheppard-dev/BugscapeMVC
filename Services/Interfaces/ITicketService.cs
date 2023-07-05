using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface ITicketService
    {
        // CRUD methods
        public Task AddNewTicketAsync(Ticket ticket);
        public Task UpdateTicketAsync(Ticket ticket);
        public Task<Ticket?> GetTicketByIdAsync(int ticketId);
        public Task ArchiveTicketAsync(Ticket ticket);

        public Task AssignTicketAsync(int ticketId, string userId);
        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsPriorityAsync(int companyId, string priorityName);
        public Task<List<Ticket>> GetAllTicketsStatusAsync(int companyId, string statusName);
        public Task<List<Ticket>> GetAllTicketsTypeAsync(int companyId, string typeName);
        public Task<List<AppUser>> GetTicketDeveloperAsync(int ticketId);
        public Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId);
        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);
        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int companyId);
        public Task<List<Ticket>> GetProjectTicketsByUserIdAsync(string userId, int companyId);
        public Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId);
        public Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId);
        public Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId);

        public Task<int?> LookupTicketPriorityIdAsync(string priorityName);
        public Task<int?> LookupTicketStatusIdAsync(string statusName);
        public Task<int?> LookupTicketTypeIdAsync(string typeName);
    }
}