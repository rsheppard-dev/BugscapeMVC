using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity;
using BugscapeMVC.Services.Interfaces;
using BugscapeMVC.Extensions;
using BugscapeMVC.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BugscapeMVC.Controllers
{
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IInviteService _inviteService;
        private readonly IEmailSender _emailService;
        private readonly IProjectService _projectService;

        public InvitesController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IInviteService inviteService,
            IEmailSender emailService,
            IProjectService projectService
        ) 
        {
            _context = context;
            _userManager = userManager;
            _inviteService = inviteService;
            _emailService = emailService;
            _projectService = projectService;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        [Authorize(Roles = nameof(Roles.Admin))]
        // GET: Invites/Create
        public async Task<IActionResult> Create(int? projectId)
        {   
            string? userId = _userManager.GetUserId(User);
            int? companyId = User.Identity?.GetCompanyId();

            if (userId is null || companyId is null) return NotFound();

            var roles = Enum.GetValues(typeof(Roles))
                .Cast<Roles>()
                .Where(role => role != Roles.Demo_User)
                .Select(role => new
                {
                    Value = role,
                    Text = role.ToString().Replace("_", " ")
                });

            var projects = (await _projectService.GetAllProjectsByCompanyAsync(companyId.Value))
                .OrderBy(project => project.Name)
                .ToList();
            
            ViewData["Projects"] = new SelectList(projects, "Id", "Name", projectId);
            ViewData["Roles"] = new SelectList(roles, "Value", "Text", nameof(Roles.Submitter));
            
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InvitorId,CompanyId,InviteeEmail,InviteeFirstName,InviteeLastName,CompanyToken,InviteDate,IsValid,Message,Role")] Invite invite)
        {
            AppUser invitor =  await _userManager.GetUserAsync(User) ?? throw new Exception("Invitor not found.");
            Company company = await _context.Companies.FindAsync(invitor.CompanyId) ?? throw new Exception("Company not found.");

            invite.InvitorId = invitor.Id;
            invite.CompanyId = company.Id;

            if (ModelState.IsValid)
            {
                await _inviteService.AddNewInviteAsync(invite);

                var callbackUrl = Url.Action(
                    "Join", // Action name
                    "Invites", // Controller name
                    new
                    { 
                        token = invite.CompanyToken,
                        email = invite.InviteeEmail,
                        id = invite.CompanyId
                    }, // Route values
                    Request.Scheme); // Protocol

                if (callbackUrl is not null)
                {
                    await _emailService.SendEmailAsync(invite.InviteeEmail!, "Join our team.", invite.Message + $"<p>Click this unique link within 7 days to accept the invite to join {company.Name}'s team.</p><p><a href='{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(callbackUrl)}'>Accept invite</a>.</p>");
                }

                return RedirectToAction(nameof(Index));
            }
            
            return View(invite);
        }

        // GET: Invites/Join/{token, email, id}
        public async Task<IActionResult> Join(Guid token, string email, int id)
        {
            Invite? invite = await _inviteService.GetInviteAsync(token, email, id);
            Company? company = await _context.Companies.FindAsync(id);

            if (invite is null || company is null) return NotFound();

            bool isValid = invite.IsValid;

            invite.IsValid = await _inviteService.ValidateInviteCodeAsync(token);

            if (isValid != invite.IsValid)
                await _inviteService.UpdateInviteAsync(invite);

            JoinViewModel model = new()
            {
                Invite = invite
            };

            return View(model);
        }

        // POST: Invites/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(JoinViewModel model)
        {
            Invite? invite = await _inviteService.GetInviteAsync(model.Invite.Id, model.Invite.CompanyId);
            
            if (invite is not null || ModelState.IsValid)
            {
                try
                {
                    AppUser member = new()
                    {
                        UserName = model.Invite.InviteeEmail,
                        Email = model.Invite.InviteeEmail,
                        FirstName = model.Invite.InviteeFirstName,
                        LastName = model.Invite.InviteeLastName,
                        EmailConfirmed = true,
                        CompanyId = model.Invite.CompanyId
                    };

                    var result = await _userManager.CreateAsync(member, model.Password);

                    if (invite?.Role != null)
                    {
                        await _userManager.AddToRoleAsync(member, invite.Role.ToString());
                    }

                    if (invite?.ProjectId is not null && invite.Role != Roles.Admin)
                    {
                        if (invite.Role == Roles.Project_Manager)
                        {
                            await _projectService.AddProjectManagerAsync(member.Id, invite.ProjectId.Value);
                        }
                        else
                        {
                            await _projectService.AddUserToProjectAsync(member.Id, invite.ProjectId.Value);
                        }
                    }

                    if (result.Succeeded)
                    {
                        bool joined = await _inviteService.AcceptInviteAsync(invite?.CompanyToken, member.Id, member.CompanyId);

                        return RedirectToPage("/Identity/Account/Login", new { email = member.Email, message = $"You have successfully joined the team." });
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return View(model);
        }

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Invites == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Invites'  is null.");
            }
            var invite = await _context.Invites.FindAsync(id);
            if (invite != null)
            {
                _context.Invites.Remove(invite);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
          return (_context.Invites?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
