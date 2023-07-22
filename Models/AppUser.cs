using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BugscapeMVC.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string? FullName { get { return  $"{FirstName} {LastName}"; } }

        [NotMapped]
        public IFormFile? AvatarFormFile { get; set; }

        [Display(Name = "Avatar File Name")]
        public string? AvatarFileName { get; set; }

         [Display(Name = "File Extension")]
        public string? AvatarContentType { get; set; }

        public byte[]? AvatarFileData { get; set; }

         [Display(Name = "Company")]
        public int CompanyId { get; set; }

        // navigation properties
        public virtual Company? Company { get; set; }
        
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}