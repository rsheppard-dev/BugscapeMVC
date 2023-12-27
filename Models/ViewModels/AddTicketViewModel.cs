using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugscapeMVC.Models.ViewModels
{
    public class AddTicketViewModel
    {
        public Ticket Ticket { get; set; } = new Ticket
        {
            Title = string.Empty,
            Description = string.Empty
        };

        public SelectList? ProjectList { get; set; }

        public SelectList? StatusList { get; set; }
        
        public SelectList? TicketTypes { get; set; }

        public SelectList? PriorityList { get; set; }
        
    }
}