using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity;
using BugscapeMVC.Extensions;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BugscapeMVC.Models.ViewModels;

namespace BugscapeMVC.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjectService _projectService;
        private readonly ILookupService _lookupService;
        private readonly ITicketService _ticketService;
        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<AppUser> userManager, IProjectService projectService, ILookupService lookupService, ITicketService ticketService, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _fileService = fileService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            string? userId = _userManager.GetUserId(User);
            int? companyId = User.Identity?.GetCompanyId();

            if (userId is null || companyId is null) return NoContent();

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId.Value);

            return View(tickets);
        }

        // GET: Tickets/AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId.Value);

            if (User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
            {
                return View(tickets.Where(ticket => ticket.Archived == false));
            }

            return View(tickets);
        }

        // GET: Tickets/ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId.Value);

            return View(tickets);
        }

        // GET: Tickets/UnassignedTickets
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]

        public async Task<IActionResult> UnassignedTickets()
        {
            int? companyId = User.Identity?.GetCompanyId();
            string? userId = _userManager.GetUserId(User);

            if (companyId is null || userId is null) return NoContent();

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId.Value);

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }

            List<Ticket> pmTickets = new();

            foreach (Ticket ticket in tickets)
            {
                if (await _projectService.IsAssignedProjectManagerAsync(userId, ticket.ProjectId))
                {
                    pmTickets.Add(ticket);
                }
            }

            return View(pmTickets);
        }

        // GET: Tickets/AssignDeveloper
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int id)
        {
            AssignDeveloperViewModel model = new()
            {
                Ticket = await _ticketService.GetTicketByIdAsync(id)
            };

            if (model.Ticket is null) return NotFound();

            model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(Roles.Developer)), "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId is not null && model.Ticket is not null)
            {
                await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);
            }

            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket?.Id });
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(ticket);
        }

        // POST: Tickets/AddTicketComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DateTime.Now;

                    await _ticketService.AddTicketCommentAsync(ticketComment);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return RedirectToAction("Details", new { id = ticketComment.TicketId });
        }

        // POST: Tickets/AddTicketAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,TicketId,Description,FormFile")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile is not null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;

                ticketAttachment.Created = DateTime.Now;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                statusMessage = "Success: New attachment added to ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";
            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, statusMessage });
        }

        // GET: Tickets/ShowFile
        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);

            if (ticketAttachment is null) return NotFound();

            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName);

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return File(fileData, $"application/{ext}");
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
