using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class TicketType
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Type Name")]
        public required string Name { get; set; }
    }
}