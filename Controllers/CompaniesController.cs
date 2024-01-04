using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using BugscapeMVC.Models.Enums;
using BugscapeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BugscapeMVC.Extensions;

namespace BugscapeMVC.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public CompaniesController(ApplicationDbContext context, UserManager<AppUser> userManager, ICompanyInfoService companyInfoService)
        {
            _context = context;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
              return _context.Companies != null ? 
                          View(await _context.Companies.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var company = await _companyInfoService.GetCompanyInfoByIdAsync(id);

            if (company is null) return NotFound();

            return View(company);
        }

        // GET: Companies/Edit/5
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var company = await _companyInfoService.GetCompanyInfoByIdAsync(id);

            if (company is null) return NotFound();

            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from over-posting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = nameof(Roles.Admin))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company)
        {
            if (id != company.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Details), new {id = company.Id});
            }

            return View(company);
        }

        // GET: Companies/Delete/5
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [Authorize(Roles = nameof(Roles.Admin))]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompanyLogo([Bind("LogoFormFile")] Company model)
        {
            try
            {
                int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception("Unable to find company.");
                IFormFile imageFormFile = model.LogoFormFile ?? throw new Exception("Unable to find image.");

                bool result = await _companyInfoService.UpdateCompanyLogoAsync(companyId.Value, imageFormFile);

                return result ?
                    RedirectToAction(nameof(Details), new {id = companyId}) :
                    View();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> DeleteCompanyLogo()
        {
            try
            {
                int? companyId = User.Identity?.GetCompanyId() ?? throw new Exception("Unable to find company.");

                bool result = await _companyInfoService.DeleteCompanyLogoAsync(companyId.Value);

                return result ?
                    RedirectToAction(nameof(Details), new {id = companyId}) :
                    View();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CompanyExists(int id)
        {
          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
