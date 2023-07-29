using BugscapeMVC.Models;

namespace BugscapeMVC.Services.Interfaces
{
    public interface IProjectService
    {
        public Task AddNewProjectAsync(Project project);
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);
        public Task<bool> AddUserToProjectAsync(string userId, int projectId);
        public Task ArchiveProjectAsync(Project project);
        public Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId);
        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName);
        public Task<List<AppUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        public Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId);
        public Task<List<AppUser>> GetDevelopersOnProjectAsync(int projectId);
        public Task<AppUser?> GetProjectManagerAsync(int projectId);
        public Task<List<AppUser>> GetProjectMembersByRoleAsync(int projectId, string role);
        public Task<Project?> GetProjectByIdAsync(int projectId, int companyId);
        public Task<List<AppUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);
        public Task<List<Project>> GetUserProjectsAsync(string userId);
        public Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId);
        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        public Task<int?> LookupProjectPriorityIdAsync(string priorityName);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task RemoveUsersFromProjectByRoleAsync(string role, int projectId);
        public Task RemoveUserFromProjectAsync(string userId, int projectId);
        public Task RestoreProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);
    }
}