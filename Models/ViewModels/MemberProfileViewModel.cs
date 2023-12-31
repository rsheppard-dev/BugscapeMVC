namespace BugscapeMVC.Models.ViewModels
{
    public class MemberProfileViewModel
    {
        public AppUser Member { get; set; } = new AppUser();
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}