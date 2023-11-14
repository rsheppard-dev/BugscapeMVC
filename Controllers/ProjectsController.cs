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
        private readonly UserManager<AppUser> _userManager;

        public ProjectsController(IRoleService roleService, ILookupService lookupService, IFileService fileService, IProjectService projectService, UserManager<AppUser> userManager, ICompanyInfoService companyInfoService, ITicketService ticketService)
        {
            _roleService = roleService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _ticketService = ticketService;
        }

        // GET: Projects/MyProjects
        [HttpGet]
        public async Task<IActionResult> MyProjects(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            string? userId = _userManager.GetUserId(User);

            if (userId is null) return NotFound();

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            // if search arguement
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
                ViewBag.Search = search;
            }

            projects = Sort(projects, sortBy, order);

            // pagination
            if (page < 1) page = 1;

            int totalProjects = projects.Count;

            Pagination pagination = new(totalProjects, page, limit);

            int skip = (page - 1) * limit;

            List<Project> data = projects.Skip(skip).Take(pagination.ResultsPerPage).ToList();

            ViewBag.Sort = new { sortBy, order, limit };
            ViewBag.Pagination = pagination;

            return View(data);
        }

        // GET: Projects
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            List<Project> projects;
            
            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.Project_Manager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId.Value);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId.Value);
            }

            // if search arguement
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
                ViewBag.Search = search;
            }

            projects = Sort(projects, sortBy, order);

            // pagination
            if (page < 1) page = 1;

            int totalProjects = projects.Count;

            Pagination pagination = new(totalProjects, page, limit);

            int skip = (page - 1) * limit;

            List<Project> data = projects.Skip(skip).Take(pagination.ResultsPerPage).ToList();

            ViewBag.Sort = new { sortBy, order, limit };
            ViewBag.Pagination = pagination;

            return View(data);
        }

        // GET: Projects/ArchivedProjects
        [HttpGet]
        public async Task<IActionResult> ArchivedProjects(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId.Value);

            // if search arguement
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
                ViewBag.Search = search;
            }

            projects = Sort(projects, sortBy, order);

            // pagination
            if (page < 1) page = 1;

            int totalProjects = projects.Count;

            Pagination pagination = new(totalProjects, page, limit);

            int skip = (page - 1) * limit;

            List<Project> data = projects.Skip(skip).Take(pagination.ResultsPerPage).ToList();

            ViewBag.Sort = new { sortBy, order, limit };
            ViewBag.Pagination = pagination;

            return View(data);
        }

        // GET: Projects/UnassignedProjects
        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> UnassignedProjects(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId.Value);

            // if search arguement
            if (!string.IsNullOrEmpty(search))
            {
                projects = Search(projects, search);
                ViewBag.Search = search;
            }

            projects = Sort(projects, sortBy, order);

            // pagination
            if (page < 1) page = 1;

            int totalProjects = projects.Count;

            Pagination pagination = new(totalProjects, page, limit);

            int skip = (page - 1) * limit;

            List<Project> data = projects.Skip(skip).Take(pagination.ResultsPerPage).ToList();

            ViewBag.Sort = new { sortBy, order, limit };
            ViewBag.Pagination = pagination;

            return View(data);
        }

        // GET: Projects/AssignPM/5
        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> AssignPM(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            AssignPMViewModel model = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id, companyId.Value),
                Project_Managers = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(Roles.Project_Manager), companyId.Value), "Id", "FullName")
            };

            return View(model);
        }

        // POST: Projects/AssignPM
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Project_ManagerId) && model.Project is not null)
            {
                await _projectService.AddProject_ManagerAsync(model.Project_ManagerId, model.Project.Id);

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

            List<AppUser> developers = await _roleService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId.Value);
            List<AppUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId.Value);

            List<AppUser> companyMembers = developers.Concat(submitters)
                .OrderBy(member => member.LastName)
                .ThenBy(member => member.FirstName)
                .ToList();

            List<string> projectMembers = project.Members.Select(member => member.Id).ToList();

            AssignMembersViewModel model = new()
            {
                Project = project,
                Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers)
            }; 

            return View(model);
        }

        // POST: Projects/AssignMembers
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Project_Manager)}")]
        public async Task<IActionResult> AssignMembers(AssignMembersViewModel model)
        {
            if (model.SelectedUsers is not null && model.Project is not null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                    .Select(member => member.Id)
                    .ToList();

                // remove previous members from projects
                foreach (string member in memberIds)
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);

                // add updated members to project
                foreach (string member in model.SelectedUsers)
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);

                // return user to project details
                return RedirectToAction("Details", new { id = model.Project.Id });
            }

            return RedirectToAction("AssignMembers", new { id = model.Project?.Id });
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
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
                        await _projectService.AddProject_ManagerAsync(model.PmId, model.Project.Id);
                    }

                    return RedirectToAction("MyProjects");
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

            AddProjectWithPMViewModel model = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value),
                PMList = new SelectList(await _roleService.GetUsersInRoleAsync(Roles.Project_Manager.ToString(), companyId.Value), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
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
                        await _projectService.AddProject_ManagerAsync(model.PmId, model.Project.Id);
                    }

                    return RedirectToAction("MyProjects");
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

        [HttpPost]
        public IActionResult SortProjects([FromBody]List<Project> projects, int? page, int? limit, string sortBy = "title", string order = "asc")
        {
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = order;

            projects = Sort(projects, sortBy, order);

            return PartialView("_ProjectsTablePartial", new PaginatedList<Project>(projects, page ?? 1, limit ?? 10));
        }

        private static List<Project> Sort(List<Project> projects, string sortBy, string order = "asc")
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
                                        projects.OrderBy(p => p.Members.Select(m => m.FullName).FirstOrDefault()).ToList() :
                                        projects.OrderByDescending(p => p.Members.Select(m => m.FullName).FirstOrDefault()).ToList(),
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
