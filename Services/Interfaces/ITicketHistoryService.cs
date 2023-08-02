using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface ITicketHistoryService
    {
        Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);
        Task AddHistoryAsync(int ticketId, string model, string userId);
        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId);
        Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);
    }
}