using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Models;
using BugscapeMVC.Extensions;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Services.Interfaces;
using BugscapeMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ILookupService _lookupService;
        private readonly IFileService _fileService;
        private readonly IProjectService _projectService;
        private readonly ICompanyInfoService _companyInfoService;
        private readonly ITicketService _ticketService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public ProjectsController(
            IRoleService roleService,
            ILookupService lookupService,
            IFileService fileService,
            IProjectService projectService,
            UserManager<AppUser> userManager,
            ICompanyInfoService companyInfoService,
            ITicketService ticketService,
            INotificationService notificationService
        )
        {
            _roleService = roleService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _ticketService = ticketService;
            _notificationService = notificationService;
        }

        // GET: Projects/MyProjects
        [HttpGet]
        public async Task<IActionResult> MyProjects(int page = 1, string search = "", string order = "desc", string sortBy = "startdate", int limit = 10)
        {
            string? userId = _userManager.GetUserId(User);

            if (userId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
            }

            projects = Sort(projects, sortBy, order);

            return View(new PaginatedList<Project>(projects, page, limit));
        }

        // GET: Projects
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "", string order = "desc", string sortBy = "startdate", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Project> projects;
            
            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.Project_Manager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId.Value);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId.Value);
            }

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
            }

            projects = Sort(projects, sortBy, order);

            return View(new PaginatedList<Project>(projects, page, limit));
        }

        // GET: Projects/ArchivedProjects
        [HttpGet]
        public async Task<IActionResult> ArchivedProjects(int page = 1, string search = "", string order = "desc", string sortBy = "startdate", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId.Value);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
            }

            projects = Sort(projects, sortBy, order);

            return View(new PaginatedList<Project>(projects, page, limit));
        }

        // GET: Projects/UnassignedProjects
        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> UnassignedProjects(int page = 1, string search = "", string order = "desc", string sortBy = "startdate", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId.Value);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
                ViewBag.Search = search;
            }

            projects = Sort(projects, sortBy, order);

            return View(new PaginatedList<Project>(projects, page, limit));
        }

        // GET: Projects/AssignPM/5
        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> AssignPM(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            Project? project = await _projectService.GetProjectByIdAsync(id, companyId.Value);

            if (project is null) return NotFound();

            string? currentPmId = (await _projectService.GetProjectManagerAsync(id))?.Id;

            AssignPMViewModel model = new()
            {
                Project = project,
                ProjectManagers = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(Roles.Project_Manager), companyId.Value), "Id", "FullName", currentPmId)
            };

            return View(model);
        }

        // POST: Projects/AssignPM
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.ProjectManagerId) && model.Project is not null)
            {
                await _projectService.AddProjectManagerAsync(model.ProjectManagerId, model.Project.Id);
                var project = await _projectService.GetProjectByIdAsync(model.Project.Id, User.Identity?.GetCompanyId() ?? throw new Exception("Company ID not valid."));

                Notification notification = new()
                {
                    Title = "New Project Assignment",
                    Message = $"You have been assigned as the Project Manager for: {project?.Name}.",
                    RecipientId = model.ProjectManagerId,
                };

                await _notificationService.AddNotificationAsync(notification);
                
                if (!User.IsInRole(nameof(Roles.Demo_User)))
                {
                    _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                }

                return RedirectToAction(nameof(Details), new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignPM), new { id = model.Project?.Id });
        }

        // GET: Projects/AssignMembers/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> AssignMembers(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            Project? project = await _projectService.GetProjectByIdAsync(id, companyId.Value);

            if (project is null) return NotFound();

            List<AppUser> allDevelopers = await _roleService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId.Value);
            List<AppUser> allSubmitters = await _roleService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId.Value);

            allDevelopers = allDevelopers
                .OrderBy(member => member.LastName)
                .ToList();

            allSubmitters = allSubmitters
                .OrderBy(member => member.FullName)
                .ToList();

            List<AppUser> selectedDevelopers = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(Roles.Developer));
            List<AppUser> selectedSubmitters = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(Roles.Submitter));

            List<AppUser> availableDevelopers = allDevelopers.Except(selectedDevelopers).ToList();
            List<AppUser> availableSubmitters = allSubmitters.Except(selectedSubmitters).ToList();

            selectedDevelopers = selectedDevelopers.OrderBy(member => member.FullName).ToList();
            selectedSubmitters = selectedSubmitters.OrderBy(member => member.FullName).ToList();

            List<string> projectMemberIds = selectedDevelopers
                .Concat(selectedSubmitters)
                .Select(member => member.Id)
                .ToList();

            AssignMembersViewModel model = new()
            {
                Project = project,
                AllDevelopers = new MultiSelectList(allDevelopers, "Id", "FullName"),
                AllSubmitters = new MultiSelectList(allSubmitters, "Id", "FullName"),
                AvailableDevelopers = new MultiSelectList(availableDevelopers, "Id", "FullName"),
                SelectedDevelopers = new MultiSelectList(selectedDevelopers, "Id", "FullName"),
                AvailableSubmitters = new MultiSelectList(availableSubmitters, "Id", "FullName"),
                SelectedSubmitters = new MultiSelectList(selectedSubmitters, "Id", "FullName"),
                SelectedUsers = projectMemberIds
            };

            return View(model);
        }

        // POST: Projects/AssignMembers
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> AssignMembers(AssignMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                    .Select(member => member.Id)
                    .ToList();

                // remove previous members from projects
                foreach (string member in memberIds)
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);

                // add the selected members to the project
                foreach (string memberId in model.SelectedUsers ?? new List<string>())
                {
                    // if memberId is null, skip to the next memberId
                    if (memberId == null) continue;

                    // add user to project
                    await _projectService.AddUserToProjectAsync(memberId, model.Project.Id);

                    // check if the user was already a member
                    if (!memberIds.Contains(memberId))
                    {
                        // send notification to new members
                        Notification notification = new()
                        {
                            Title = "New Project Assignment",
                            Message = $"You have been assigned to the project: {model.Project.Name}.",
                            RecipientId = memberId,
                        };

                        await _notificationService.AddNotificationAsync(notification);
                        
                        if (!User.IsInRole(nameof(Roles.Demo_User)))
                        {
                            _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                        }
                    }
                }

                return RedirectToAction("Details", new { id = model.Project.Id });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                return View(model); // Return the view with the same model to display the errors
            }
        }

        // GET: Projects/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (id is null || companyId is null)
                return NotFound();

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value);

            if (project is null) return NotFound();

            return View(project);
        }

        // GET: Projects/Create
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Create()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            AddProjectWithPMViewModel model = new()
            {
                PMList = new SelectList(await _roleService.GetUsersInRoleAsync(Roles.Project_Manager.ToString(), companyId.Value), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: Projects/Create
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (model is not null)
            {
                int? companyId = User.Identity?.GetCompanyId();

                if (companyId is null) return RedirectToAction("Create");

                if (!ModelState.IsValid)
                {
                    model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(Roles.Project_Manager.ToString(), companyId.Value), "Id", "FullName");
                    model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");
    
                    return View(model);
                }

                try
                {
                    if (model.Project?.ImageFormFile is not null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    if (model.Project is null) return RedirectToAction("Create");

                    model.Project.CompanyId = companyId.Value;

                    await _projectService.AddNewProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);

                        // send notification to PM
                        Notification notification = new()
                        {
                            Title = "Project Assignment",
                            Message = $"You have been assigned as the Project Manager for: {model.Project.Name}.",
                            RecipientId = model.PmId,
                        };

                        await _notificationService.AddNotificationAsync(notification);
                        if (!User.IsInRole(nameof(Roles.Demo_User)))
                        {
                            _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                        }
                    }

                    return User.IsInRole(nameof(Roles.Admin)) ?
                        RedirectToAction("Index") :
                        RedirectToAction("MyProjects");
                }
                catch (Exception)
                { 
                    throw;
                }
            }
  
            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Edit(int? id)
        {       
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value);

            if (project is null) return NotFound();

            AddProjectWithPMViewModel model = new()
            {
                Project = project,
                PMList = new SelectList(await _roleService.GetUsersInRoleAsync(Roles.Project_Manager.ToString(), companyId.Value), "Id", "FullName", (await _projectService.GetProjectManagerAsync(id.Value))?.Id),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model is not null && model.Project is not null)
            {
                try
                {
                    if (model.Project?.ImageFormFile is not null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    if (model.Project is null) return RedirectToAction("Edit");

                    await _projectService.UpdateProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                    return User.IsInRole(nameof(Roles.Admin)) ?
                        RedirectToAction("Index") :
                        RedirectToAction("MyProjects");
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool projectExists = await ProjectExistsAsync(model.Project.Id);

                    if (!projectExists)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
  
            return RedirectToAction("Edit");
        }

        // GET: Projects/Archive/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (id == null || companyId == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null)
            {
                return Problem("Company ID is null.");
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, companyId.Value);

            if (project is null)
            {
                return Problem("Project not found.");
            }
        
            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> Restore(int? id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (id == null || companyId == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null)
            {
                return Problem("Company ID is null.");
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, companyId.Value);

            if (project is null)
            {
                return Problem("Project not found.");
            }
        
            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProjectExistsAsync(int id)
        {
            try
            {
                int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception("Company ID not valid.");
                return (await _projectService.GetAllProjectsByCompanyAsync(companyId.Value)).Any(project => project.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectsByQuery(string query, int page = 1, int limit = 4, string sortBy = "title", string order = "asc", string? memberId = null)
        {
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = order;

            int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception("Company ID not valid.");
            string? userId = _userManager.GetUserId(User) ?? throw new Exception("User ID not valid.");

            List<Project> projects = query switch
            {
                "getAllProjects" => await _projectService.GetAllProjectsByCompanyAsync(companyId.Value),
                "getMyProjects" => await _projectService.GetUserProjectsAsync(userId),
                "getUnassignedProjects" => await _projectService.GetUnassignedProjectsAsync(companyId.Value),
                "getArchivedProjects" => await _projectService.GetArchivedProjectsByCompanyAsync(companyId.Value),
                "getProjectsByMember" => memberId is not null ? await _projectService.GetUserProjectsAsync(memberId) : new List<Project>(),
                _ => await _projectService.GetAllProjectsByCompanyAsync(companyId.Value)
            };

            projects = Sort(projects, sortBy, order);

            return PartialView("_ProjectsTablePartial", new PaginatedList<Project>(projects, page, limit));
        }

        private List<Project> Sort(List<Project> projects, string sortBy, string order = "desc")
        {
            if (projects is null)
            {
                return new List<Project>();
            }

            projects = (sortBy?.ToLower()) switch
            {
                "startdate" => order == "asc" ?
                                        projects.OrderBy(p => p.StartDate).ToList() :
                                        projects.OrderByDescending(p => p.StartDate).ToList(),
                "enddate" => order == "asc" ?
                                        projects.OrderBy(p => p.EndDate).ToList() :
                                        projects.OrderByDescending(p => p.EndDate).ToList(),
                "priority" => order == "asc" ?
                                        projects.OrderBy(p => p.ProjectPriority?.Name).ToList() :
                                        projects.OrderByDescending(p => p.ProjectPriority?.Name).ToList(),
                "pm" => order == "asc" ?
                                        projects.OrderBy(p => _projectService.GetProjectManagerAsync(p.Id).Result?.FullName).ToList() :
                                        projects.OrderByDescending(p => _projectService.GetProjectManagerAsync(p.Id).Result?.FullName).ToList(),
                _ => order == "asc" ?
                                        projects.OrderBy(p => p.Name).ToList() :
                                        projects.OrderByDescending(p => p.Name).ToList(),
            };

            return projects;
        }

        private static List<Project> Search(List<Project> projects, string search)
        {
            if (projects is null)
            {
                return new List<Project>();
            }
            
            return projects
                .Where(p => 
                    (p.Name?.ToLower().Contains(search.ToLower()) ?? false) || 
                    (p.Description?.ToLower().Contains(search) ?? false))
                .ToList();
        }
    }
}
