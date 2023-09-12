namespace BugscapeMVC.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Company? Company { get; set; }
        public List<Project>? Projects { get; set; }
        public PaginatedList<Ticket>? Tickets { get; set; }
        public List<AppUser>? Members { get; set; }
    }
}
