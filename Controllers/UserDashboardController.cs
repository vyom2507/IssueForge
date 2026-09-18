using System.Security.Claims;
using IssueForge.Data;
using IssueForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueForge.Controllers;

[Authorize]
public class UserDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(
                "Index",
                "AdminDashboard");
        }

        if (User.IsInRole("SupportAgent"))
        {
            return RedirectToAction(
                "Index",
                "SupportAgent");
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return Challenge();
        }

        var tickets = await _context.Tickets
            .Where(t => t.CreatedByUserId == currentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var viewModel = new UserDashboardViewModel
        {
            TotalTickets = tickets.Count,

            OpenTickets = tickets.Count(
                t => t.Status == TicketStatus.Open),

            AssignedTickets = tickets.Count(
                t => t.Status == TicketStatus.Assigned),

            InProgressTickets = tickets.Count(
                t => t.Status == TicketStatus.InProgress),

            ResolvedTickets = tickets.Count(
                t => t.Status == TicketStatus.Resolved),

            ClosedTickets = tickets.Count(
                t => t.Status == TicketStatus.Closed),

            RecentTickets = tickets
                .Take(5)
                .ToList()
        };

        return View(viewModel);
    }
}