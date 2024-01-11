using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugscapeMVC.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public NotificationsController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService
        )
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Notifications
        public async Task<IActionResult> Index(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            var notifications = await _context.Notifications.Include(n => n.Recipient).Include(n => n.Sender).Include(n => n.Ticket).OrderByDescending(n => n.Created).ToListAsync();

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                notifications = Search(notifications, search);
            }

            notifications = Sort(notifications, sortBy, order);

            return View(notifications.ToList());
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null ) return NotFound();

            var notification = await _notificationService.GetNotificationByIdAsync(id.Value);

            if (notification is null) return NotFound();

            // check if the current user is the recipient of the notification
            if (notification.RecipientId == _userManager.GetUserId(User))
            {
                notification.Viewed = true;
                await _notificationService.UpdateNotificationAsync(notification);
            }


            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
            return View();
        }

        // POST: Notifications/Create
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TicketId,Title,Message,Created,RecipientId,SenderId,Viewed")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Title,Message,Created,RecipientId,SenderId,Viewed")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Notifications == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Notifications'  is null.");
            }
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static List<Notification> Sort(List<Notification> notifications, string sortBy = "date", string order = "asc")
        {
            notifications = sortBy.ToLower() switch
            {
                "title" => order == "asc" ?
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

        private bool NotificationExists(int id)
        {
          return (_context.Notifications?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
