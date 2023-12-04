using BugscapeMVC.Models;

namespace BugscapeMVC.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public PaginatedList<Ticket> Tickets { get; set; } = new PaginatedList<Ticket>(new List<Ticket>(), 1, 10);
        public PaginatedList<Project> Projects { get; set; } = new PaginatedList<Project>(new List<Project>(), 1, 10);
        public PaginatedList<AppUser> Members { get; set; } = new PaginatedList<AppUser>(new List<AppUser>(), 1, 10);
        public int NumberOfResults => Tickets.TotalItems + Projects.TotalItems + Members.TotalItems;

       public List<dynamic> Tabs
        {
            get
            {
                var tabs = new List<dynamic>
                {
                    new { Label = "Projects", Projects.TotalItems, Id = "projects-button", Controls = "projects", DataTab = "projects" },
                    new { Label = "Tickets", Tickets.TotalItems, Id = "tickets-button", Controls = "tickets", DataTab = "tickets" },
                    new { Label = "Members", Members.TotalItems, Id = "members-button", Controls = "members", DataTab = "members" }
                };

                var maxCount = tabs.Max(tab => tab.TotalItems);

                return tabs.Select(tab => new { tab.Label, tab.TotalItems, tab.Id, tab.Controls, tab.DataTab, IsActive = tab.TotalItems == maxCount })
                        .OrderByDescending(tab => tab.TotalItems)
                        .ToList<dynamic>();
            }
        }

        public string ActiveTab => Tabs.FirstOrDefault(tab => tab.IsActive)?.DataTab ?? "projects";
    }
}