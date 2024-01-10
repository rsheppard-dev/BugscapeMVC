using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset? Updated { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset? ResolvedDate { get; set; }

        [DefaultValue(false)]
        public bool Archived { get; set; }

        [DefaultValue(false)]
        [Display(Name = "Archived By Project")]
        public bool ArchivedByProject { get; set; }

        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }

        [Display(Name = "Ticket Priority")]
        public int TicketPriorityId { get; set; }

        [Display(Name = "Ticket Status")]
        public int TicketStatusId { get; set; }

        [Display(Name = "Ticket Owner")]
        public string? OwnerUserId { get; set; }

        [Display(Name = "Ticket Developer")]
        public string? DeveloperUserId { get; set; }

        // navigation properties
        public virtual Project? Project { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual AppUser? OwnerUser { get; set; }
        public virtual AppUser? DeveloperUser { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}