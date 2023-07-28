using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity;
using BugscapeMVC.Extensions;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;

namespace BugscapeMVC.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjectService _projectService;
        private readonly ILookupService _lookupService;
        private readonly ITicketService _ticketService;
        private readonly UserManager<AppUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<AppUser> userManager, IProjectService projectService, ILookupService lookupService, ITicketService ticketService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            string? userId = _userManager.GetUserId(User);
            int? companyId = User.Identity?.GetCompanyId();

            if (userId is null || companyId is null) return NotFound();

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId.Value), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            string? userId = _userManager.GetUserId(User);
            int? companyId = User.Identity?.GetCompanyId();

            if (userId is null || companyId is null) return NotFound();

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.Now;
                ticket.OwnerUserId = userId;
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(TicketStatuses.New))).Value;   

                await _ticketService.AddNewTicketAsync(ticket);

                // todo: add ticket history and notification

                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId.Value), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }
            
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool ticketExists = await TicketExistsAsync(ticket.Id);

                    if (!ticketExists)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                 
                 // todo: add ticket history
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id is null || _context.Tickets is null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket is null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket is not null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id is null || _context.Tickets is null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket is null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket is not null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TicketExistsAsync(int id)
        {
          int? companyId = User.Identity?.GetCompanyId();

          if (companyId is null) return false;

          return (await _ticketService.GetAllTicketsByCompanyAsync(companyId.Value))
            .Any(ticket => ticket.Id == id);
        }
    }
}
