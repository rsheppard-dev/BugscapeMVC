using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AssignMembersViewModel
    {
        public Project Project { get; set; } = new Project();
        public MultiSelectList AvailableSubmitters { get; set; } = new MultiSelectList(new[] { new { Id = "", FullName = "" } }, "Id", "FullName");
        public MultiSelectList SelectedSubmitters { get; set; } = new MultiSelectList(new[] { new { Id = "", FullName = "" } }, "Id", "FullName");
        public MultiSelectList AvailableDevelopers { get; set; } = new MultiSelectList(new[] { new { Id = "", FullName = "" } }, "Id", "FullName");
        public MultiSelectList SelectedDevelopers { get; set; } = new MultiSelectList(new[] { new { Id = "", FullName = "" } }, "Id", "FullName");
        public List<string>? SelectedUsers { get; set; }
    }
}