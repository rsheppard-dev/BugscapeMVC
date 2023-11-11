using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class ProjectService : IProjectService
    {
        #region Properties
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;
        #endregion

        #region Constructor
        public ProjectService(ApplicationDbContext context, IRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }
        #endregion

        #region  Add New Project
        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {     
                throw;
            }
        }
        #endregion

        #region Add Project Manager
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            AppUser? currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM is not null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing current PM.\nError: {ex.Message}");
                    return false;
                }
            }

            try
            {
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding new PM.\nError: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Add User to Project
        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            AppUser? user = await _context.Users.FindAsync(userId);

            if (user is null) return false;

            Project? project = await _context.Projects.FindAsync(projectId);

            if (project is null) return false;

            // check user is not a member of project
            if (!await IsUserOnProjectAsync(userId, projectId))
            {
                try
                {
                    project.Members.Add(user);
                    await _context.SaveChangesAsync();
                    
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return false;
        }
        #endregion

        #region Archive Project
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                // archive all tickets attached to project
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;

                    _context.Update(ticket);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {   
                throw;
            }
        }
        #endregion

        #region Get All Project Members Except PM
        public async Task<List<AppUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<AppUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
            List<AppUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
            List<AppUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

            List<AppUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

            return teamMembers;
        }
        #endregion

        #region Get All Projects By Company
        public async Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = await _context.Projects
                .Where(project => project.CompanyId == companyId && project.Archived == false)
                .Include(project => project.Members)
                .Include(project => project.ProjectPriority)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.Comments)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.Attachments)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.History)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.Notifications)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.DeveloperUser)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.OwnerUser)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.TicketPriority)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.TicketStatus)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.TicketType)
                .ToListAsync();

            return projects;
        }
        #endregion

        #region Get All Projects By Priority
        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompanyAsync(companyId);
            int? priorityId = await LookupProjectPriorityIdAsync(priorityName);

            return projects.Where(project => project.ProjectPriorityId == priorityId).ToList();
        }
        #endregion

        #region Get Archived Projects By Company
        public async Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = await _context.Projects
                .Where(project => project.CompanyId == companyId && project.Archived)
                .Include(project => project.Members)
                .Include(project => project.ProjectPriority)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.Comments)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.Attachments)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.History)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.Notifications)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.DeveloperUser)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.OwnerUser)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.TicketPriority)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.TicketStatus)
                .Include(project => project.Tickets)
                    .ThenInclude(ticket => ticket.TicketType)
                .ToListAsync();

            return projects;
        }
        #endregion

        #region Get Developers on Project
        public Task<List<AppUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Get Project By ID
        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                    .Include(project => project.ProjectPriority)
                    .Include(project => project.Members)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.TicketPriority)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.TicketStatus)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.TicketType)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.DeveloperUser)
                    .Include(project => project.Tickets)
                        .ThenInclude(ticket => ticket.OwnerUser)
                    .FirstOrDefaultAsync(project => project.Id == projectId && project.CompanyId == companyId);

                return project;
            }
            catch (Exception)
            {               
                throw;
            }
        }
        #endregion

        #region Get Project Manager
        public async Task<AppUser?> GetProjectManagerAsync(int projectId)
        {
            Project? project = await _context.Projects
                .Include(project => project.Members)
                .FirstOrDefaultAsync(project => project.Id == projectId);

            if (project is null) return null;

            foreach (AppUser member in project.Members)
            {
                if (await _roleService.HasRoleAsync(member, Roles.ProjectManager.ToString()))
                {
                    return member;
                }
            }

            return null;
        }
        #endregion

        #region Get Project Members By Role
        public async Task<List<AppUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            List<AppUser> members = new();

            Project? project = await _context.Projects
                .Include(project => project.Members)
                .FirstOrDefaultAsync(project => project.Id == projectId);

            if (project is null) return members;

            foreach (AppUser user in project.Members)
            {
                if (await _roleService.HasRoleAsync(user, role))
                {
                    members.Add(user);
                }
            }

            return members;
        }
        #endregion

        #region Get Unassigned Projects
        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();

            try
            {
                projects = await _context.Projects
                    .Include(project => project.ProjectPriority)
                    .Where(project => project.CompanyId == companyId)
                    .ToListAsync();

                foreach (Project project in projects)
                {
                    if ((await GetProjectMembersByRoleAsync(project.Id, nameof(Roles.ProjectManager))).Count == 0)
                    {
                        result.Add(project);
                    }
                }

                return result;
            }
            catch (Exception)
            {     
                throw;
            }
        }
        #endregion

        #region Get User Projects
        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> projects = (await _context.Users
                    .Include(user => user.Projects)
                        .ThenInclude(project => project.Company)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Members)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Tickets)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Tickets)
                            .ThenInclude(ticket => ticket.OwnerUser)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Tickets)
                            .ThenInclude(tickets => tickets.DeveloperUser)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.ProjectPriority)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Tickets)
                            .ThenInclude(tickets => tickets.TicketPriority)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Tickets)
                            .ThenInclude(tickets => tickets.TicketStatus)
                    .Include(user => user.Projects)
                        .ThenInclude(projects => projects.Tickets)
                            .ThenInclude(tickets => tickets.TicketType)
                    .FirstOrDefaultAsync(user => user.Id == userId))?
                    .Projects.ToList() ?? new List<Project>();
                
                return projects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to get user projects.\nException: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Get Users Not On Project
        public async Task<List<AppUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<AppUser> users = await _context.Users
                .Where(user => user.Projects.All(project => project.Id != projectId))
                .ToListAsync();

            return users.Where(user => user.CompanyId == companyId).ToList();
        }
        #endregion

        #region Is Assigned Project Manager
        public async Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                string? ProjectManagerId = (await GetProjectManagerAsync(projectId))?.Id;

                return userId == ProjectManagerId;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Is User On Project?
        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project? project = await _context.Projects
                .Include(project => project.Members)
                .FirstOrDefaultAsync(project => project.Id == projectId);

            bool result = false;

            if (project is not null)
            {
                result = project.Members.Any(member => member.Id == userId);
            }

            return result;
        }
        #endregion

        #region Lookup Project Priority ID
        public async Task<int?> LookupProjectPriorityIdAsync(string priorityName)
        {
            int? priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(priority => priority.Name == priorityName))?.Id;

            return priorityId;
        }
        #endregion

        #region Remove Project Manager
        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project? project = await _context.Projects
                .Include(project => project.Members)
                .FirstOrDefaultAsync(project => project.Id == projectId);
            
            if (project is null) return;

            try
            {
                foreach (AppUser member in project.Members)
                {
                    if (await _roleService.HasRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion

        #region Remove User From Project
        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                AppUser? user = await _context.Users.FindAsync(userId);

                if (user is null) return;

                Project? project = await _context.Projects.FindAsync(projectId);

                if (project is null) return;

                // check user is a member of project
                if (await IsUserOnProjectAsync(userId, projectId))
                {
                    project.Members.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to remove user from project.\nException: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Remove Users From Project By Role
        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                Project? project = await _context.Projects.FindAsync(projectId);

                if (project is null) return;

                List<AppUser> members = await GetProjectMembersByRoleAsync(projectId, role);

                foreach (AppUser user in members)
                {
                    project.Members.Remove(user);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to remove users from project.\nException: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Restore Project
        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                // restore all tickets attached to project
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;

                    _context.Update(ticket);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {   
                throw;
            }
        }
        #endregion

        #region Update Project
        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}