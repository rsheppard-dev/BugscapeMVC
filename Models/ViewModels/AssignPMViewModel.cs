using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project Project { get; set; } = new Project();
        public SelectList ProjectManagers { get; set; } = new SelectList(new[] { new { Id = "", FullName = "" } }, "Id", "FullName");
        public string? ProjectManagerId { get; set; }
    }
}