using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugscapeMVC.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [Display(Name = "Ticket")]
        public int TicketId { get; set; }

        [Display(Name = "File Date")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Team Member")]
        public required string UserId { get; set; }

        [Display(Name = "File Description")]
        public string? Description { get; set; }
        
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? FormFile { get; set; }

        [Display(Name = "File Name")]
        public required string FileName { get; set; }

        public required byte[] FileData { get; set; }

        [Display(Name = "File Extension")]
        public required string FileContentType { get; set; }

        // navigation properties
        public virtual Ticket? Ticket { get; set; }
        public virtual AppUser? User { get; set; }
    }
}