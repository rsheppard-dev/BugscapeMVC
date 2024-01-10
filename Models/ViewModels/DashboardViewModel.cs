using BugscapeMVC.Models.ChartModels;

namespace BugscapeMVC.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Company? Company { get; set; }
        public List<Project>? Projects { get; set; }
        public List<Ticket>? Tickets { get; set; }
        public List<AppUser>? Members { get; set; }
        public ChartJsData? ChartData { get; set; }
    }
}
