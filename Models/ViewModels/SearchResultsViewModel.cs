namespace BugscapeMVC.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<AppUser> Members { get; set; } = new List<AppUser>();
        public int NumberOfResults => Tickets.Count + Projects.Count + Members.Count;

       public List<dynamic> Tabs
        {
            get
            {
                var tabs = new List<dynamic>
                {
                    new { Label = "Projects", Projects.Count, Id = "projects-button", Controls = "projects", DataTab = "projects" },
                    new { Label = "Tickets", Tickets.Count, Id = "tickets-button", Controls = "tickets", DataTab = "tickets" },
                    new { Label = "Members", Members.Count, Id = "members-button", Controls = "members", DataTab = "members" }
                };

                var maxCount = tabs.Max(tab => tab.Count);

                return tabs.Select(tab => new { tab.Label, tab.Count, tab.Id, tab.Controls, tab.DataTab, IsActive = tab.Count == maxCount })
                        .OrderByDescending(tab => tab.Count)
                        .ToList<dynamic>();
            }
        }

        public string ActiveTab => Tabs.FirstOrDefault(tab => tab.IsActive)?.DataTab!;
    }
}