using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Priority Name")]
        public required string Name { get; set; }
    }
}