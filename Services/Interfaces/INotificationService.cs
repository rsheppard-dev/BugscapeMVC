using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface INotificationService
    {
        public Task AddNotificationAsync(Notification notification);
        public Task<List<Notification>> GetReceivedNotificationsAsync(string userId);
        public Task<List<Notification>> GetSentNotificationsAsync(string userId);
        public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);
        public Task SendMembersEmailNotificationsAsync(Notification notification, List<AppUser> members);
        public Task<bool> SendEmailNotificationAync(Notification notification, string emailSubject);
    }
}