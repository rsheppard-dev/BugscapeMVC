using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BugscapeMVC.Models;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Models.ChartModels;
using BugscapeMVC.Extensions;
using BugscapeMVC.Services.Interfaces;
using BugscapeMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BugscapeMVC.Controllers;

public class HomeController : Controller
{
    private readonly ICompanyInfoService _companyInfoService;
    private readonly IProjectService _projectService;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ICompanyInfoService companyInfoService, IProjectService projectService, UserManager<AppUser> userManager)
    {
        _companyInfoService = companyInfoService;
        _projectService = projectService;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (string.IsNullOrEmpty(_userManager.GetUserId(User)))
            return View();
        else
            return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Dashboard()
    {
        DashboardViewModel model = new();

        int? companyId = User.Identity?.GetCompanyId();
        
        if (companyId is null) return NotFound();

        model.Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId.Value) ?? throw new Exception("Company details not found.");
        
        model.Projects = (await _companyInfoService.GetAllProjectsAsync(companyId.Value))
            .Where(project => !project.Archived)
            .ToList();     
        model.Tickets = model.Projects
            .SelectMany(project => project.Tickets)
            .Where(ticket => !ticket.Archived)
            .OrderBy(ticket => ticket.Title)
            .ToList();
        model.Members = model.Company.Members.ToList();

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> SearchResults(string? tab, string searchString = "", int projectsPage = 1, int ticketsPage = 1, int membersPage = 1, int limit = 10)
    {
        try
        {
            SearchResultsViewModel results = new();

            ViewBag.SearchString = searchString;
            ViewBag.Limit = limit;
            ViewBag.Page = new
            {
                Projects = projectsPage,
                Tickets = ticketsPage,
                Members = membersPage
            };

            if (string.IsNullOrEmpty(searchString)) return View(results);

            int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception();

            List<Project> projects = (await _companyInfoService.GetAllProjectsAsync(companyId.Value))
                .Where(p => p.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            List<Ticket> tickets = (await _companyInfoService.GetAllTicketsAsync(companyId.Value))
                .Where(t => t.Title?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            List<AppUser> members = (await _companyInfoService.GetAllMembersAsync(companyId.Value))
                .Where(m => m.FirstName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true || m.LastName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            results.Projects = new PaginatedList<Project>(projects, projectsPage, limit);
            results.Tickets = new PaginatedList<Ticket>(tickets, ticketsPage, limit);
            results.Members = new PaginatedList<AppUser>(members, membersPage, limit);

            ViewBag.ActiveTab = tab ?? results.ActiveTab;

            return View(results);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost]
    public async Task<JsonResult> GglProjectTickets()
    {
        int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception();

        List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId.Value);

        List<object> chartData = new()
        {
            new object[] { "ProjectName", "TicketCount" }
        };

        foreach (Project prj in projects)
        {
            if (prj.Name is not null)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count });
            }
        }

        return Json(chartData);
    }

    [HttpPost]
    public async Task<JsonResult> GglProjectPriority()
    {
        int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception("No company ID.");

        List<object> chartData = new()
        {
            new object[] { "Priority", "Count" }
        };


        foreach (string priority in Enum.GetNames(typeof(Priorities)))
        {
            int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId.Value, priority)).Count;
            chartData.Add(new object[] { priority, priorityCount });
        }

        return Json(chartData);
    }


    [HttpPost]
    public async Task<JsonResult> AmCharts()
    {

        AmChartData amChartData = new();
        List<AmItem> amItems = new(); 

        int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception();

        List<Project> projects = (await _companyInfoService.GetAllProjectsAsync(companyId.Value)).Where(p=>p.Archived == false).ToList();

        foreach(Project project in projects)
        {
            AmItem item = new()
            {
                Project = project.Name,
                Tickets = project.Tickets.Count,
                Developers = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(Roles.Developer))).Count
            };

            amItems.Add(item);  
        }

        amChartData.Data = amItems.ToArray();   


        return Json(amChartData.Data);
    }

    [HttpPost]
    public async Task<JsonResult> PlotlyBarChart()
    {
        PlotlyBarData plotlyData = new();
        List<PlotlyBar> barData = new();

        int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception();

        List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId.Value);

        //Bar One
        PlotlyBar barOne = new()
        {
            X = projects.Select(p => p.Name!).ToArray(),
            Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
            Name = "Tickets",
            Type =  "bar"
        };

        //Bar Two
        PlotlyBar barTwo = new()
        {
            X = projects.Select(p => p.Name!).ToArray(),
            Y = projects.Select(async p=> (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(Roles.Developer))).Count).Select(c=>c.Result).ToArray(),
            Name = "Developers",
            Type = "bar"
        };

        barData.Add(barOne);    
        barData.Add(barTwo);

        plotlyData.Data = barData;

        return Json(plotlyData);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
