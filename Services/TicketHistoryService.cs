using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class TicketHistoryService : ITicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        public TicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket? oldTicket, Ticket newTicket, string userId)
        {
            // if new ticket has been added
            if (oldTicket is null && newTicket is not null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = "New ticket created."
                };

                try
                {
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                { 
                    throw;
                }
            }
            else
            {
                // check if ticket title has changed
                if (oldTicket is not null &&
                    newTicket is not null &&
                    oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket title: {newTicket.Title}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                // check if ticket description has changed.
                if (oldTicket is not null &&
                    newTicket is not null &&
                    oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket description: {newTicket.Description}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                // check if ticket priority has changed.
                if (oldTicket?.TicketPriority is not null &&
                    newTicket?.TicketPriority is not null &&
                    oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Priority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket priority: {newTicket.TicketPriority.Name}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                // check if ticket status has changed.
                if (oldTicket?.TicketStatus is not null &&
                    newTicket?.TicketStatus is not null &&
                    oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Status",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket status: {newTicket.TicketStatus.Name}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                // check if ticket type has changed.
                if (oldTicket?.TicketType is not null &&
                    newTicket?.TicketType is not null &&
                    oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Type",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket type: {newTicket.TicketType.Name.Replace("_", " ")}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                // check changes to ticket developer
                if (newTicket?.DeveloperUser is not null &&
                    newTicket?.DeveloperUser?.FullName is not null &&
                    oldTicket?.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket?.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket developer: {newTicket.DeveloperUser.FullName}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {                  
                    throw;
                }
            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId) ?? throw new Exception("Ticket not found.");

                string description = model.ToLower().Replace("ticket", "");

                if (model == nameof(TicketComment))
                {
                    var latestComment = ticket.Comments
                        .OrderByDescending(comment => comment.Created)
                        .Select(comment => comment.Comment)
                        .FirstOrDefault();

                    description = latestComment ?? string.Empty;
                }
                else
                {
                    description = $"New {description} added to ticket.";
                }       

                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    Property = model,
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = description   
                };

                await _context.TicketHistories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                    .Include(company => company.Projects)
                        .ThenInclude(project => project.Tickets)
                            .ThenInclude(ticket => ticket.History)
                                .ThenInclude(history => history.User)
                    .FirstOrDefaultAsync(company => company.Id == companyId))?.Projects
                    .ToList() ?? new();

                List<Ticket> tickets = projects.SelectMany(project => project.Tickets).ToList();

                List<TicketHistory> ticketHistories = tickets.SelectMany(ticket => ticket.History).ToList();

                return ticketHistories;
            }
            catch (Exception)
            {     
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                    .Where(project => project.CompanyId == companyId)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.History)
                                .ThenInclude(history => history.User)
                    .FirstOrDefaultAsync(project => project.Id == projectId);

                if (project is null) return new List<TicketHistory>();

                List<TicketHistory> ticketHistories = project.Tickets
                    .SelectMany(ticket => ticket.History)
                    .ToList();

                return ticketHistories;
            }
            catch (Exception)
            {               
                throw;
            }
        }
    }
}
