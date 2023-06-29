using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugscapeMVC.Data;
using BugscapeMVC.Models;

namespace BugscapeMVC.Controllers
{
    public class TicketStatusesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketStatusesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketStatuses
        public async Task<IActionResult> Index()
        {
              return _context.TicketStatuses != null ? 
                          View(await _context.TicketStatuses.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.TicketStatuses'  is null.");
        }

        // GET: TicketStatuses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TicketStatuses == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketStatus == null)
            {
                return NotFound();
            }

            return View(ticketStatus);
        }

        // GET: TicketStatuses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TicketStatuses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] TicketStatus ticketStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticketStatus);
        }

        // GET: TicketStatuses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TicketStatuses == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            if (ticketStatus == null)
            {
                return NotFound();
            }
            return View(ticketStatus);
        }

        // POST: TicketStatuses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] TicketStatus ticketStatus)
        {
            if (id != ticketStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketStatusExists(ticketStatus.Id))
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
            return View(ticketStatus);
        }

        // GET: TicketStatuses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TicketStatuses == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketStatus == null)
            {
                return NotFound();
            }

            return View(ticketStatus);
        }

        // POST: TicketStatuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TicketStatuses == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TicketStatuses'  is null.");
            }
            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            if (ticketStatus != null)
            {
                _context.TicketStatuses.Remove(ticketStatus);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketStatusExists(int id)
        {
          return (_context.TicketStatuses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
