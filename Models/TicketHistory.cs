using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [Display(Name = "Ticket")]
        public int TicketId { get; set; }

        [Display(Name = "Updated Item")]
        public required string Property { get; set; }

        [Display(Name = "Previous")]
        public string? OldValue { get; set; }

        [Display(Name = "Current")]
        public required string NewValue { get; set; }

        [Display(Name = "Date Modified")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Description of Change")]
        public string? Description { get; set; }

        [Display(Name = "Team Member")]
        public required string UserId { get; set; }

        // navigation properties
        public virtual Ticket? Ticket { get; set; }
        public virtual AppUser? User { get; set; }
    }
}