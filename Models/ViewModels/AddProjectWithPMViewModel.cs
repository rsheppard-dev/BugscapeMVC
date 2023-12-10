using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AddProjectWithPMViewModel
    {
        public Project Project { get; set; } = new Project();
        public SelectList? PMList { get; set; }

        [Display(Name = "Project Manager")]
        public string? PmId { get; set; }
        public SelectList? PriorityList { get; set; }
    }
}