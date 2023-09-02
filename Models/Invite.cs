using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BugscapeMVC.Models.Enums;

namespace BugscapeMVC.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Sent")]
        public DateTimeOffset InviteDate { get; set; } = DateTimeOffset.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Join Date")]
        public DateTimeOffset? JoinDate { get; set; }

        [Display(Name = "Code")]
        public Guid CompanyToken { get; set; } = Guid.NewGuid();

        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Role")]
        public Roles Role { get; set; } = Roles.Submitter;

        [Display(Name = "Invitor")]
        public string? InvitorId { get; set; }

        [Display(Name = "Invitee")]
        public string? InviteeId { get; set; }

        [Required]
        [Display(Name = "Invitee Email")]
        [EmailAddress]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "Invitee First Name")]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Invitee Last Name")]
        public string? InviteeLastName { get; set; }

        [Display(Name = "Invite Message")]
        public string? Message { get; set; } = "We want you to join our team!";

        public bool IsValid { get; set; } = true;

        // navigation properties
        public virtual Company? Company { get; set; }
        public virtual AppUser? Invitor { get; set; }
        public virtual AppUser? Invitee { get; set; }
    }
}