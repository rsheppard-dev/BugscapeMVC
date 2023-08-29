using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugscapeMVC.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public string? Name { get; set; }

        [Display(Name = "Company Description")]
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? LogoFormFile { get; set; }

        [Display(Name = "Logo File Name")]
        public string? LogoFileName { get; set; }

         [Display(Name = "File Extension")]
        public string? LogoContentType { get; set; }

        public byte[]? LogoFileData { get; set; }

        // navigation properties
        public virtual ICollection<AppUser> Members { get; set; } = new HashSet<AppUser>();

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}