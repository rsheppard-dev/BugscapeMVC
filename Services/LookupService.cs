using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _context;

        public LookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                var ticketTypes = await _context.TicketTypes.ToListAsync();

                return ticketTypes.Select(tt => new TicketType
                {
                    Id = tt.Id,
                    Name = tt.Name.Replace("_", " "),
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}