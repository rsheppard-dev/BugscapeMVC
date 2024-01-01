namespace BugscapeMVC.Services.Interfaces
{
    public interface ITicketAttachmentService
    {
        public Task<bool> DeleteAttachmentAsync(int ticketAttachmentId);
    }
}