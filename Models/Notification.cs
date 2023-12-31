using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Display(Name = "Ticket")]
        public int? TicketId { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Message { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        [Required]
        [Display(Name = "Recipient")]
        public required string RecipientId { get; set; }

        [Required]
        [Display(Name = "Sender")]
        public required string SenderId { get; set; }

        [DefaultValue(false)]
        [Display(Name = "Has been viewed")]
        public bool Viewed { get; set; }

        // navigation properties
        public virtual Ticket? Ticket { get; set; }

        public virtual AppUser? Recipient { get; set; }

        public virtual AppUser? Sender { get; set; }

    }
}