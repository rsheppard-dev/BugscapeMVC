using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Extensions;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Services.Interfaces;
using BugscapeMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BugscapeMVC.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;
        private readonly ILookupService _lookupService;
        private readonly IFileService _fileService;
        private readonly IProjectService _projectService;
        private readonly ICompanyInfoService _companyInfoService;
        private readonly UserManager<AppUser> _userManager;

        public ProjectsController(ApplicationDbContext context, IRoleService roleService, ILookupService lookupService, IFileService fileService, IProjectService projectService, UserManager<AppUser> userManager, ICompanyInfoService companyInfoService)
        {
            _context = context;
            _roleService = roleService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects/MyProjects
        public async Task<IActionResult> MyProjects()
        {
            string? userId = _userManager.GetUserId(User);

            if (userId is null) return NotFound();
            
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        // GET: Projects/AllProjects
        public async Task<IActionResult> AllProjects()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            List<Project> projects;
            
            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId.Value);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId.Value);
            }

            return View(projects);
        }

        // GET: Projects/ArchivedProjects
        public async Task<IActionResult> ArchivedProjects()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();
            
            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId.Value);

            return View(projects);
        }

        // GET: Projects/UnassignedProjects
        public async Task<IActionResult> UnassignedProjects()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            List<Project> projects = new();

            projects = await _projectService.GetUnassignedProjectsAsync(companyId.Value);

            return View(projects);
        }

        // GET: Projects/AssignPM
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> AssignPM(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NoContent();

            AssignPMViewModel model = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id, companyId.Value),
                ProjectManagers = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId.Value), "Id", "FullName")
            };

            return View(model);
        }

        // POST: Projects/AssignPM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.ProjectManagerId) && model.Project is not null)
            {
                await _projectService.AddProjectManagerAsync(model.ProjectManagerId, model.Project.Id);

                return RedirectToAction(nameof(Details), new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignPM), new { id = model.Project?.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (id is null || _context.Projects is null || companyId is null)
                return NotFound();

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value);

            if (project is null) return NotFound();

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            AddProjectWithPMViewModel model = new()
            {
                PMList = new SelectList(await _roleService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId.Value), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception)
                { 
                    throw;
                }
            }
  
            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {       
            int? companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            AddProjectWithPMViewModel model = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id.Value, companyId.Value),
                PMList = new SelectList(await _roleService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId.Value), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model is not null)
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

                    return RedirectToAction("Index");
                }
                catch (Exception)
                { 
                    throw;
                }
            }
  
            return RedirectToAction("Edit");
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (id == null || _context.Projects == null || companyId == null)
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
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (_context.Projects is null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

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
        public async Task<IActionResult> Restore(int? id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (id == null || _context.Projects == null || companyId == null)
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
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int? companyId = User.Identity?.GetCompanyId();

            if (_context.Projects is null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

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

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
