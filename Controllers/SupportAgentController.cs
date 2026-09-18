using System.Security.Claims;
using IssueForge.Data;
using IssueForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueForge.Controllers;

[Authorize(Roles = "SupportAgent")]
public class SupportAgentController : Controller
{
    private readonly ApplicationDbContext _context;

    public SupportAgentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return Challenge();
        }

        var tickets = await _context.Tickets
            .Where(t => t.AssignedToUserId == currentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var viewModel = new SupportAgentDashboardViewModel
        {
            TotalAssignedTickets = tickets.Count,

            AssignedTickets = tickets.Count(
                t => t.Status == TicketStatus.Assigned),

            InProgressTickets = tickets.Count(
                t => t.Status == TicketStatus.InProgress),

            ResolvedTickets = tickets.Count(
                t => t.Status == TicketStatus.Resolved),

            ClosedTickets = tickets.Count(
                t => t.Status == TicketStatus.Closed),

            Tickets = tickets
        };

        return View(viewModel);
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
                t.AssignedToUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int ticketId,
        TicketStatus status)
    {
        var allowedStatuses = new[]
        {
            TicketStatus.Assigned,
            TicketStatus.InProgress,
            TicketStatus.Resolved,
            TicketStatus.Closed
        };

        if (!allowedStatuses.Contains(status))
        {
            TempData["ErrorMessage"] =
                "Invalid ticket status.";

            return RedirectToAction(nameof(Index));
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.Id == ticketId &&
                t.AssignedToUserId == currentUserId);

        if (ticket == null)
        {
            return NotFound();
        }

        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Ticket #{ticket.Id} status updated to {status}.";

        return RedirectToAction(nameof(Index));
    }
}