using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AssignMembersViewModel
    {
        public Project? Project { get; set; }
        public MultiSelectList? Users { get; set; }
        public List<string>? SelectedUsers { get; set; }
    }
}