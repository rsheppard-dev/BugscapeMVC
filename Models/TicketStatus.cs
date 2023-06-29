using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status Name")]
        public required string Name { get; set; }
    }
}