using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Display(Name = "Member Comment")]
        public required string Comment { get; set; }

        [Display(Name = "Date")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Ticket")]
        public int TicketId { get; set; }

        [Display(Name = "Team Member")]
        public required string UserId { get; set; }

        // navigation properties
        public virtual Ticket? Ticket { get; set; }
        public virtual AppUser? User { get; set; }
    }
}