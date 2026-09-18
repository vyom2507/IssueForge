using System.Security.Claims;
using IssueForge.Data;
using IssueForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueForge.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TicketsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var tickets = await _context.Tickets
            .Where(t => t.CreatedByUserId == currentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.CreatedByUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Priority,DueDate")]
        Ticket ticket)
    {
        if (!ModelState.IsValid)
        {
            return View(ticket);
        }

        ticket.Status = TicketStatus.Open;
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.CreatedByUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.CreatedByUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,Priority,DueDate")]
        Ticket updatedTicket)
    {
        if (id != updatedTicket.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(updatedTicket);
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.CreatedByUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        ticket.Title = updatedTicket.Title;
        ticket.Description = updatedTicket.Description;
        ticket.Priority = updatedTicket.Priority;
        ticket.DueDate = updatedTicket.DueDate;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.CreatedByUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.CreatedByUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        _context.Tickets.Remove(ticket);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}