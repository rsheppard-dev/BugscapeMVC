using Microsoft.AspNetCore.Mvc;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugscapeMVC.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using BugscapeMVC.Models.Enums;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ITicketService _ticketService;
        private readonly ICompanyInfoService _companyInfoService;
        private readonly UserManager<AppUser> _userManager;

        public NotificationsController(
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            ITicketService ticketService,
            ICompanyInfoService companyInfoService
        )
        {
            _notificationService = notificationService;
            _ticketService = ticketService;
            _companyInfoService = companyInfoService;
            _userManager = userManager;
        }

        // GET: Notifications
        public async Task<IActionResult> Index(int page = 1, string search = "", string order = "desc", string sortBy = "date", int limit = 10)
        {
            string? userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId)) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Notification> notifications = await _notificationService.GetReceivedNotificationsAsync(userId);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                notifications = Search(notifications, search);
            }

            notifications = Sort(notifications, sortBy, order);

            return View(new PaginatedList<Notification>(notifications, page, limit));
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string? userId = _userManager.GetUserId(User);

            if (id is null || userId is null) return NotFound();

            var notification = await _notificationService.GetNotificationByIdAsync(id.Value);

            if (notification is null) return NotFound();

            if (userId != notification.RecipientId && userId != notification.SenderId) return NotFound();

            // check if the current user is the recipient of the notification
            if (notification.RecipientId == _userManager.GetUserId(User))
            {
                notification.Viewed = true;
                await _notificationService.UpdateNotificationAsync(notification);
            }


            return View(notification);
        }

        // GET: Notifications/Create
        public async Task<IActionResult> Create(string? recipientId, string? subject, int? ticketId)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();
            
            List<AppUser> members = await _companyInfoService.GetAllMembersAsync(companyId.Value) ?? new List<AppUser>();
            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId.Value))
                .OrderBy(t => t.Title)
                .ToList()
                ?? new List<Ticket>();

            // remove the current user from the list of recipients
            members = members
                .Where(m => m.Id != _userManager.GetUserId(User))
                .OrderBy(m => m.FullName)
                .ToList();

            ViewBag.RecipientId = new SelectList(members, "Id", "FullName", recipientId);
            ViewBag.TicketId = new SelectList(tickets, "Id", "Title", ticketId);
            ViewBag.Subject = subject;

            return View();
        }

        // POST: Notifications/Create
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TicketId,Title,Message,Created,RecipientId,SenderId")] Notification notification, string? subject, string? recipientId, int? ticketId)
        {
            if (ModelState.IsValid)
            {
                await _notificationService.AddNotificationAsync(notification);

                if (!User.IsInRole(nameof(Roles.Demo_User)))
                {
                    _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);
                }

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Create), new { subject, recipientId, ticketId });
        }

        // POST: Notifications/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            
            return RedirectToAction(nameof(Index));
        }

        private static List<Notification> Sort(List<Notification> notifications, string sortBy = "date", string order = "asc")
        {
            notifications = sortBy.ToLower() switch
            {
                "subject" => order == "asc" ?
                    notifications.OrderBy(n => n.Title).ToList() :
                    notifications.OrderByDescending(n => n.Title).ToList(),
                "sender" => order == "asc" ?
                    notifications.OrderBy(n => n.Sender?.FullName).ToList() :
                    notifications.OrderByDescending(n => n.Sender?.FullName).ToList(),
                _ => order == "asc" ?
                    notifications.OrderBy(n => n.Created).ToList() :
                    notifications.OrderByDescending(n => n.Created).ToList(),
            };

            return notifications;
        }

        private static List<Notification> Search(List<Notification> notifications, string search)
        {
            if (notifications is null) return new List<Notification>();
            
            return notifications
                .Where(n => (n.Sender?.FullName?.ToLower().Contains(search.ToLower()) ?? false) ||
                    (n.Message?.ToLower().Contains(search.ToLower()) ?? false) ||
                    (n.Title?.ToLower().Contains(search.ToLower()) ?? false))
                .ToList();
        }
    }
}
