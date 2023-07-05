using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;

        public ProjectService(ApplicationDbContext context, IRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task AddNewProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

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

        public async Task ArchiveProjectAsync(Project project)
        {
            project.Archived = true;
            _context.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AppUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<AppUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
            List<AppUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
            List<AppUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

            List<AppUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

            return teamMembers;
        }

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

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompanyAsync(companyId);
            int? priorityId = await LookupProjectPriorityIdAsync(priorityName);

            return projects.Where(project => project.ProjectPriorityId == priorityId).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = await GetAllProjectsByCompanyAsync(companyId);
            List<Project> result = projects.Where(project => project.Archived).ToList();

            return result;
        }

        public Task<List<AppUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            Project? project = await _context.Projects
                .Include(project => project.ProjectPriority)
                .Include(project => project.Members)
                .Include(project => project.Tickets)
                .FirstOrDefaultAsync(project => project.Id == projectId && project.CompanyId == companyId);

            return project;
        }

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

        public Task<List<AppUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

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

        public async Task<List<AppUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<AppUser> users = await _context.Users
                .Where(user => user.Projects.All(project => project.Id != projectId))
                .ToListAsync();

            return users.Where(user => user.CompanyId == companyId).ToList();
        }

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

        public async Task<int?> LookupProjectPriorityIdAsync(string priorityName)
        {
            int? priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(priority => priority.Name == priorityName))?.Id;

            return priorityId;
        }

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

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}