using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugscapeMVC.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Sent")]
        public DateTimeOffset InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Join Date")]
        public DateTimeOffset JoinDate { get; set; }

        [Display(Name = "Code")]
        public Guid CompanyToken { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Invitor")]
        public required string InvitorId { get; set; }

        [Display(Name = "Invitee")]
        public required string InviteeId { get; set; }

        [Display(Name = "Invitee Email")]
        [EmailAddress]
        public required string InviteeEmail { get; set; }

        [Display(Name = "Invitee First Name")]
        public required string InviteeFirstName { get; set; }

        [Display(Name = "Invitee Last Name")]
        public required string InviteeLastName { get; set; }

        [DefaultValue(true)]
        public bool IsValid { get; set; }

        // navigation properties
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }
        public virtual AppUser? Invitor { get; set; }
        public virtual AppUser? Invitee { get; set; }
    }
}