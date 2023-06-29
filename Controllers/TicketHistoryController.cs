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
    public class TicketHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketHistory
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketHistory.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketHistory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TicketHistory == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistory
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketHistory == null)
            {
                return NotFound();
            }

            return View(ticketHistory);
        }

        // GET: TicketHistory/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: TicketHistory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TicketId,Property,OldValue,NewValue,Created,Description,UserId")] TicketHistory ticketHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketHistory.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketHistory.UserId);
            return View(ticketHistory);
        }

        // GET: TicketHistory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TicketHistory == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistory.FindAsync(id);
            if (ticketHistory == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketHistory.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketHistory.UserId);
            return View(ticketHistory);
        }

        // POST: TicketHistory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Property,OldValue,NewValue,Created,Description,UserId")] TicketHistory ticketHistory)
        {
            if (id != ticketHistory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketHistoryExists(ticketHistory.Id))
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
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketHistory.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketHistory.UserId);
            return View(ticketHistory);
        }

        // GET: TicketHistory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TicketHistory == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistory
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketHistory == null)
            {
                return NotFound();
            }

            return View(ticketHistory);
        }

        // POST: TicketHistory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TicketHistory == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TicketHistory'  is null.");
            }
            var ticketHistory = await _context.TicketHistory.FindAsync(id);
            if (ticketHistory != null)
            {
                _context.TicketHistory.Remove(ticketHistory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketHistoryExists(int id)
        {
          return (_context.TicketHistory?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
