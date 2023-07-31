using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project? Project { get; set; }
        public SelectList? ProjectManagers { get; set; }
        public string? ProjectManagerId { get; set; }
    }
}