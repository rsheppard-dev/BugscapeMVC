using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Priority Name")]
        public required string Name { get; set; }
    }
}