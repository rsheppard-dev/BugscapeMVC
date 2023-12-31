using BugscapeMVC.Extensions;
using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Models.ViewModels;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ILogger<MembersController> _logger;
        private readonly IMemberService _memberService;
        private readonly ICompanyInfoService _companyService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IProjectService _projectService;
        private readonly ITicketService _ticketService;

        public MembersController(ILogger<MembersController> logger, IMemberService memberService, ICompanyInfoService companyService, UserManager<AppUser> userManager, IRoleService roleService, IProjectService projectService, ITicketService ticketService)
        {
            _logger = logger;
            _memberService = memberService;
            _companyService = companyService;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
        }

        public async Task<IActionResult> Index(int page = 1, string search = "", string order = "asc", string sortBy = "name", int limit = 10)
        {
            var companyId = User.Identity?.GetCompanyId();

            if (companyId is null) return NotFound();

            ViewBag.Search = search;
            ViewBag.Order = order;
            ViewBag.SortBy = sortBy;

            var members = await _companyService.GetAllMembersAsync(companyId.Value);

            // if search argument
            if (!string.IsNullOrEmpty(search))
            {
                members = Search(members, search);
            }

            members = await Sort(_userManager, members, sortBy, order);

            return View(new PaginatedList<AppUser>(members, page, limit));
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            var companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            AppUser? member = await _memberService.GetMemberByIdAsync(companyId.Value, id);

            if (member is null) return NotFound();

            var model = new MemberProfileViewModel
            {
                Member = member,
                Projects = await _projectService.GetUserProjectsAsync(member.Id),
                Tickets = await _ticketService.GetTicketsByUserIdAsync(member.Id, companyId.Value)
            };

            return View(model);
        }

        // GET: Members/Delete/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        public async Task<IActionResult> Delete(string? id)
        {
            var companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();
    
            AppUser? member = await _memberService.GetMemberByIdAsync(companyId.Value, id);

            if (member is null) return NotFound();

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            AppUser? member = await _memberService.GetMemberByIdAsync(companyId.Value, id);

            if (member is not null)
            {
                await _memberService.RemoveMemberAsync(member);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private static async Task<List<AppUser>> Sort(UserManager<AppUser> userManager, List<AppUser> members, string sortBy = "name", string order = "asc")
        {
            // Load the members and their roles into memory
            var memberRoles = new List<(AppUser Member, IList<string> Roles)>();
            foreach (var member in members)
            {
                var roles = await userManager.GetRolesAsync(member);
                memberRoles.Add((member, roles));
            }

            // Sort the members
            var sortedMembers = sortBy.ToLower() switch
            {
                "role" => order == "asc" ?
                    memberRoles.OrderBy(m => m.Roles.FirstOrDefault()).ToList() :
                    memberRoles.OrderByDescending(m => m.Roles.FirstOrDefault()).ToList(),
                "email" => order == "asc" ?
                    memberRoles.OrderBy(m => m.Member.Email).ToList() :
                    memberRoles.OrderByDescending(m => m.Member.Email).ToList(),
                _ => order == "asc" ?
                    memberRoles.OrderBy(m => m.Member.FullName).ToList() :
                    memberRoles.OrderByDescending(m => m.Member.FullName).ToList(),
            };

            // Return the sorted members
            return sortedMembers.Select(m => m.Member).ToList();
        }

        private static List<AppUser> Search(List<AppUser> members, string search)
        {
            if (members is null) return new List<AppUser>();
            
            return members
                .Where(m => m.FullName?.ToLower().Contains(search.ToLower()) ?? false)
                .ToList();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}