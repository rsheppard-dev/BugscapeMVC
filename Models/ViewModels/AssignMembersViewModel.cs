using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AssignMembersViewModel
    {
        public Project Project { get; set; } = new Project();
        public MultiSelectList Users { get; set; } = new MultiSelectList(new List<AppUser>(), "Id", "FullName");
        public MultiSelectList SelectedUsers { get; set; } = new MultiSelectList(new List<AppUser>(), "Id", "FullName");
        public List<string>? SelectedUserIds { get; set; }
    }
}