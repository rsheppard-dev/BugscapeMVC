namespace BugscapeMVC.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<AppUser> Members { get; set; } = new List<AppUser>();
        public int NumberOfResults => Tickets.Count + Projects.Count + Members.Count;
    }
}