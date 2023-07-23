using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugscapeMVC.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        public required string Name { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTimeOffset StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTimeOffset EndDate { get; set; }

        [Display(Name = "Priority")]
        public int? ProjectPriorityId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        [Display(Name = "File Name")]
        public string? ImageFileName { get; set; }

        [Display(Name = "File Extension")]
        public string? ImageContentType { get; set; }

        public byte[]? ImageFileData { get; set; }

        [DefaultValue(false)]
        public bool Archived { get; set; }

        // navigation properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        
        public virtual ICollection<AppUser> Members { get; set; } = new HashSet<AppUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}