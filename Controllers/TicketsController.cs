using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity;
using BugscapeMVC.Extensions;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BugscapeMVC.Models.ViewModels;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ILookupService _lookupService;
        private readonly ITicketService _ticketService;
        private readonly IFileService _fileService;
        private readonly ITicketHistoryService _historyService;
        private readonly ITicketAttachmentService _attachmentService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public TicketsController(
            UserManager<AppUser> userManager,
            IProjectService projectService,
            ILookupService lookupService,
            ITicketService ticketService,
            IFileService fileService,
            ITicketHistoryService historyService,
            ITicketAttachmentService attachmentService,
            INotificationService notificationService
        )
        {
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _fileService = fileService;
            _historyService = historyService;
            _attachmentService = attachmentService;
            _notificationService = notificationService;
        }

        // GET: Tickets/MyTickets
        [HttpGet]
        public async Task<IActionResult> MyTickets(int page = 1, string search = "", string order = "desc", string sortBy = "date", int limit = 10)
        {
            string? userId = _userManager.GetUserId(User);
            int? companyId = User.Identity?.GetCompanyId();

            if (userId is null || companyId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                tickets = Search(tickets, search);
            }

            tickets = Sort(tickets, sortBy, order);

            return View(new PaginatedList<Ticket>(tickets, page, limit));
        }

        // GET: Tickets/AllTickets
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "", string order = "desc", string sortBy = "date", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId.Value);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                tickets = Search(tickets, search);
            }

            tickets = Sort(tickets, sortBy, order);

            if (User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
            {
                return View(new PaginatedList<Ticket>(tickets.Where(ticket => ticket.Archived == false).ToList(), page, limit));
            }

            return View(new PaginatedList<Ticket>(tickets, page, limit));
        }

        // GET: Tickets/ArchivedTickets
        [HttpGet]
        public async Task<IActionResult> ArchivedTickets(int page = 1, string search = "", string order = "desc", string sortBy = "date", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId.Value);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                tickets = Search(tickets, search);
            }

            tickets = Sort(tickets, sortBy, order);

            return View(new PaginatedList<Ticket>(tickets, page, limit));
        }

        // GET: Tickets/UnassignedTickets
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]

        public async Task<IActionResult> UnassignedTickets(int page = 1, string search = "", string order = "desc", string sortBy = "date", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();
            string? userId = _userManager.GetUserId(User);

            if (companyId is null || userId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId.Value);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                tickets = Search(tickets, search);
            }

            tickets = Sort(tickets, sortBy, order);

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(new PaginatedList<Ticket>(tickets, page, limit));
            }

            List<Ticket> pmTickets = new();

            foreach (Ticket ticket in tickets)
            {
                if (await _projectService.IsAssignedProjectManagerAsync(userId, ticket.ProjectId))
                {
                    pmTickets.Add(ticket);
                }
            }

            return View(new PaginatedList<Ticket>(pmTickets, page, limit));
        }

        // GET: Tickets/AssignDeveloper
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
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
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId is not null && model.Ticket is not null)
            {
                string? userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);

                if (userId is null || oldTicket is null) return NotFound();

                try
                {
                    await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);

                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);

                    await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);

                    // send notification
                    Notification notification = new()
                    {
                        TicketId = model.Ticket.Id,
                        Title = "Developer Assignment",
                        Message = $"You have been assigned as the developer on ticket: {newTicket.Title}.",
                        RecipientId = model.DeveloperId,
                    };

                    await _notificationService.AddNotificationAsync(notification);
                    
                    if (!User.IsInRole(nameof(Roles.Demo_User)))
                    {
                        _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                    }
                }
                catch (Exception)
                {
                    throw;
                }

                return RedirectToAction(nameof(Details), new { id = model.Ticket?.Id });
            }

            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket?.Id });
        }

        // GET: Tickets/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id, string? statusMessage)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(statusMessage)) ViewBag.StatusMessage = statusMessage;

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
                    Ticket ticket = await _ticketService.GetTicketByIdAsync(ticketComment.TicketId) ?? throw new Exception("Ticket not found.");

                    ticketComment.UserId = _userManager.GetUserId(User) ?? throw new Exception("UserId cannot be null.");
                    ticketComment.Created = DateTime.Now;

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    // add history
                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);

                    // add notification

                    var recipient = ticket?.OwnerUserId == ticketComment?.UserId ? ticket?.DeveloperUserId : ticket?.OwnerUserId;

                    Notification notification = new()
                    {
                        TicketId = ticketComment?.TicketId,
                        Title = "New Comment",
                        Message = $"A new comment has been added to ticket: {ticket?.Title}.",
                        RecipientId = recipient ?? throw new Exception("RecipientId cannot be null."),
                    };

                    await _notificationService.AddNotificationAsync(notification);
                    
                    if (!User.IsInRole(nameof(Roles.Demo_User)))
                    {
                        _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return RedirectToAction("Details", new { id = ticketComment?.TicketId });
        }

        // POST: Tickets/AddTicketAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,TicketId,Description,FormFile")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile is not null)
            {
                try
                {
                    Ticket ticket = await _ticketService.GetTicketByIdAsync(ticketAttachment.TicketId) ?? throw new Exception("Ticket not found.");

                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;

                    ticketAttachment.Created = DateTime.Now;
                    ticketAttachment.UserId = _userManager.GetUserId(User) ?? throw new Exception("UserId cannot be null.");

                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                    // add history
                    await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);

                    // add notification
                    var recipient = ticketAttachment.UserId == ticket.OwnerUserId ? ticket.DeveloperUserId : ticket.OwnerUserId;

                    Notification notification = new()
                    {
                        TicketId = ticketAttachment.TicketId,
                        Title = "New Attachment",
                        Message = $"A new attachment has been added to ticket: {ticket.Title}.",
                        SenderId = ticketAttachment.UserId,
                        RecipientId = recipient ?? throw new Exception("RecipientId cannot be null."),
                    };
                }
                catch (Exception)
                {      
                    throw;
                }

                statusMessage = "Success: New attachment added to ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";
            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, statusMessage });
        }

        // GET: Tickets/ShowFile/5
        [HttpGet]
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

        // POST: Tickets/DeleteFile/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int id)
        {
            TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);

            bool status = false;

            if (ticketAttachment is not null)
            {
                status = await _attachmentService.DeleteAttachmentAsync(ticketAttachment.Id);
            }

            string statusMessage = status ? "Success: Attachment deleted." : "Error: Failed to delete attachment.";

            return RedirectToAction("Details", new { id = ticketAttachment?.TicketId, statusMessage });
        }

        // GET: Tickets/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            string? userId = _userManager.GetUserId(User);
            int? companyId = User.Identity?.GetCompanyId();

            if (userId is null || companyId is null) return NotFound();

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["Projects"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId.Value), "Id", "Name", projectId);
            }
            else
            {
                ViewData["Projects"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name", projectId);
            }

            ViewData["Priorities"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypes"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            
            return View();
        }

        // POST: Tickets/Create
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
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
                try
                {
                    Project project = await _projectService.GetProjectByIdAsync(ticket.ProjectId, companyId.Value) ?? throw new Exception("Project not found.");

                    ticket.Created = DateTime.Now;
                    ticket.OwnerUserId = userId;
                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(TicketStatuses.New))).Value;   

                    await _ticketService.AddNewTicketAsync(ticket);

                    // add ticket history
                    var newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _historyService.AddHistoryAsync(null, newTicket, userId);

                    // add notification
                    Notification notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = "New Ticket",
                        Message = $"A new ticket for {project.Name} has been created: {ticket.Title}.",
                        RecipientId = (await _projectService.GetProjectManagerAsync(ticket.ProjectId))?.Id ?? throw new Exception("RecipientId cannot be null."),
                    };

                    await _notificationService.AddNotificationAsync(notification);
                    if (!User.IsInRole(nameof(Roles.Demo_User)))
                    {
                        _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                    }
                }
                catch (Exception)
                { 
                    throw;
                }

                if (User.IsInRole(nameof(Roles.Admin)))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(MyTickets));
                }
            }

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["Projects"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId.Value), "Id", "Name", ticket.ProjectId);
            }
            else
            {
                ViewData["Projects"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name", ticket.ProjectId);
            }

            ViewData["Priorities"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypes"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            string? userId = _userManager.GetUserId(User);

            if (userId is null || id is null) return NotFound();

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket is null) return NotFound();

            ViewData["Statuses"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["Priorities"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypes"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            string? userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId) || id != ticket.Id) return NotFound();   

            if (ModelState.IsValid)
            {
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    if (ticket.TicketStatusId == (await _ticketService.LookupTicketStatusIdAsync(nameof(TicketStatuses.Resolved))).Value)
                    {
                        ticket.ResolvedDate = DateTime.Now;
                    }
                    
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
                 
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);

                return RedirectToAction(nameof(Index));
            }

            ViewData["Statuses"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["Priorities"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypes"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Archive/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id is null)
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
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket is not null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Restore/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id is null)
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
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket is not null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult SortTickets([FromBody]List<Ticket> tickets, int? page, int? limit, string sortBy = "date", string order = "desc")
        {
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = order;

            tickets = Sort(tickets, sortBy, order);

            return PartialView("_TicketsTablePartial", new PaginatedList<Ticket>(tickets, page ?? 1, limit ?? 5));
        }

        private static List<Ticket> Sort(List<Ticket> tickets, string sortBy = "date", string order = "desc")
        {
            tickets = sortBy.ToLower() switch
            {
                "title" => order == "asc" ?
                                        tickets.OrderBy(t => t.Title).ToList() :
                                        tickets.OrderByDescending(t => t.Title).ToList(),
                "developer" => order == "asc" ?
                                        tickets.OrderBy(t => t.DeveloperUser?.FullName).ToList() :
                                        tickets.OrderByDescending(t => t.DeveloperUser?.FullName).ToList(),
                                       
                "status" => order == "asc" ?
                                        tickets.OrderBy(t => t.TicketStatus?.Name).ToList() :
                                        tickets.OrderByDescending(t => t.TicketStatus?.Name).ToList(),
                "priority" => order == "asc" ?
                                        tickets.OrderBy(t => t.TicketPriority?.Name).ToList() :
                                        tickets.OrderByDescending(t => t.TicketPriority?.Name).ToList(),
                _ => order == "asc" ?
                                        tickets.OrderBy(t => t.Created).ToList() :
                                        tickets.OrderByDescending(t => t.Created).ToList()
            };

            return tickets;
        }

        private static List<Ticket> Search(List<Ticket> tickets, string search)
        {
            if (tickets is null)
            {
                return new List<Ticket>();
            }
            
            return tickets
                .Where(p => 
                    (p.Title?.ToLower().Contains(search.ToLower()) ?? false) || 
                    (p.Description?.ToLower().Contains(search) ?? false))
                .ToList();
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
