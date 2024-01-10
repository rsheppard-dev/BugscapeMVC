namespace BugscapeMVC.Models.ChartModels
{
    public class ChartJsData
    {
        public ChartJsItem[]? Data { get; set; }
    }

    public class ChartJsItem
    {
        public DateTime Date { get; set; }
        public int TicketsSubmitted { get; set; }
        public int TicketsUpdated { get; set; }
    }
}