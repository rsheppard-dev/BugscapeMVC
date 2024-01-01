using BugscapeMVC.Data;
using BugscapeMVC.Services.Interfaces;

namespace BugscapeMVC.Services
{
    public class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly ApplicationDbContext _context;

        public TicketAttachmentService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<bool> DeleteAttachmentAsync(int ticketAttachmentId)
        {
            try
            {
                var attachment = await _context.TicketAttachments.FindAsync(ticketAttachmentId);
                
                if (attachment is null) return false;

                _context.TicketAttachments.Remove(attachment);
                _context.SaveChanges();

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