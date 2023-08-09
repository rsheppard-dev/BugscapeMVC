using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BugscapeMVC.Models;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Extensions;
using BugscapeMVC.Services.Interfaces;
using BugscapeMVC.Models.Enums;

namespace BugscapeMVC.Controllers;

public class HomeController : Controller
{
    private readonly ICompanyInfoService _companyInfoService;
    private readonly IProjectService _projectService;

    public HomeController(ICompanyInfoService companyInfoService, IProjectService projectService)
    {
        _companyInfoService = companyInfoService;
        _projectService = projectService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Dashboard()
    {
        DashboardViewModel model = new();

        int? companyId = User.Identity?.GetCompanyId();
        
        if (companyId is null) return NoContent();

        model.Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId.Value);
        model.Projects = (await _companyInfoService.GetAllProjectsAsync(companyId.Value))
            .Where(project => !project.Archived)
            .ToList();
        model.Tickets = model.Projects
            .SelectMany(project => project.Tickets)
            .Where(ticket => !ticket.Archived)
            .ToList();
        model.Members = model.Company?.Members.ToList();

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
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
            chartData.Add(new object[] { prj.Name, prj.Tickets.Count });
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
