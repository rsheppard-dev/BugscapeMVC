using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public required AppUser AppUser { get; set; }
        public required SelectList Roles { get; set; }
        public string? SelectedRole { get; set; }
    }
}