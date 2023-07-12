using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public required AppUser AppUser { get; set; }
        public required MultiSelectList Roles { get; set; }
        public List<string>? SelectedRoles { get; set; }
    }
}