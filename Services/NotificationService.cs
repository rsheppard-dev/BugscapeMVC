using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly IRoleService _roleService;
        
        public NotificationService(ApplicationDbContext context, IEmailSender emailService, IRoleService roleService)
        {
            _context = context;
            _emailService = emailService;
            _roleService = roleService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {                
                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                    .Include(notification => notification.Recipient)
                    .Include(notification => notification.Sender)
                    .Include(notification => notification.Ticket)
                        .ThenInclude(ticket => ticket!.Project)
                    .Where(notification => notification.RecipientId == userId)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception)
            {          
                throw;
            }
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                    .Include(notification => notification.Recipient)
                    .Include(notification => notification.Sender)
                    .Include(notification => notification.Ticket)
                        .ThenInclude(ticket => ticket!.Project)
                    .Where(notification => notification.SenderId == userId)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception)
            {          
                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            AppUser? recipient = await _context.Users.FindAsync(notification.RecipientId);

            if (recipient is null || recipient?.Email is null) return false;

            string email = recipient.Email;
            string message = notification.Message;

            try
            {
                await _emailService.SendEmailAsync(email, emailSubject, message);
                return true;
            }
            catch (Exception)
            {          
                throw;
            }
        }

        public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            try
            {
                List<AppUser> members = await _roleService.GetUsersInRoleAsync(role, companyId);

                foreach(AppUser member in members)
                {
                    notification.RecipientId = member.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {          
                throw;
            }
        }

        public async Task SendMembersEmailNotificationsAsync(Notification notification, List<AppUser> members)
        {
            try
            {
                foreach (AppUser member in members)
                {
                    notification.RecipientId = member.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {          
                throw;
            }
        }
    }
}