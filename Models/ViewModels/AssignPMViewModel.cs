using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project? Project { get; set; }
        public SelectList? Project_Managers { get; set; }
        public string? Project_ManagerId { get; set; }
    }
}