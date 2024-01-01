using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BugscapeMVC.Extensions;

namespace BugscapeMVC.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [Display(Name = "Ticket")]
        [Required]
        public int TicketId { get; set; }

        [Display(Name = "File Date")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Team Member")]
        public string? UserId { get; set; }

        [Display(Name = "File Description")]
        public string? Description { get; set; }
        
        [NotMapped]
        [DataType(DataType.Upload)]
        [Display(Name = "Upload Attachment")]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }

        [Display(Name = "File Name")]
        public string? FileName { get; set; }

        public byte[]? FileData { get; set; }

        [Display(Name = "File Extension")]
        public string? FileContentType { get; set; }

        // navigation properties
        public virtual Ticket? Ticket { get; set; }
        public virtual AppUser? User { get; set; }
    }
}