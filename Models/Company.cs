using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public required string Name { get; set; }

        [Display(Name = "Company Description")]
        public string? Description { get; set; }

        // navigation properties
        public virtual ICollection<AppUser> Members { get; set; } = new HashSet<AppUser>();

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}