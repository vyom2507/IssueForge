using IssueForge.Data;
using IssueForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueForge.Controllers;

[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminDashboardController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new AdminDashboardViewModel
        {
            TotalTickets = await _context.Tickets.CountAsync(),

            OpenTickets = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.Open),

            AssignedTickets = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.Assigned),

            InProgressTickets = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.InProgress),

            ResolvedTickets = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.Resolved),

            ClosedTickets = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.Closed),

            RecentTickets = await _context.Tickets
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> AllTickets()
    {
        var tickets = await _context.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var userIds = tickets
            .SelectMany(t => new[]
            {
                t.CreatedByUserId,
                t.AssignedToUserId
            })
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToList();

        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(
                u => u.Id,
                u => u.Email ?? u.UserName ?? "Unknown");

        var supportAgentUsers =
            await _userManager.GetUsersInRoleAsync("SupportAgent");

        var viewModel = new AdminTicketsViewModel
        {
            Tickets = tickets.Select(ticket =>
                new AdminTicketRowViewModel
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt,
                    DueDate = ticket.DueDate,

                    CreatedByEmail =
                        ticket.CreatedByUserId != null &&
                        users.TryGetValue(
                            ticket.CreatedByUserId,
                            out var createdByEmail)
                            ? createdByEmail
                            : "Unknown",

                    AssignedToUserId = ticket.AssignedToUserId,

                    AssignedToEmail =
                        ticket.AssignedToUserId != null &&
                        users.TryGetValue(
                            ticket.AssignedToUserId,
                            out var assignedEmail)
                            ? assignedEmail
                            : null
                })
                .ToList(),

            SupportAgents = supportAgentUsers
                .Select(user => new SupportAgentOptionViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? user.UserName ?? "Unknown"
                })
                .OrderBy(agent => agent.Email)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
        int ticketId,
        string? supportAgentId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);

        if (ticket == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(supportAgentId))
        {
            ticket.AssignedToUserId = null;

            if (ticket.Status == TicketStatus.Assigned)
            {
                ticket.Status = TicketStatus.Open;
            }

            TempData["SuccessMessage"] =
                $"Ticket #{ticket.Id} is now unassigned.";
        }
        else
        {
            var supportAgent =
                await _userManager.FindByIdAsync(supportAgentId);

            if (supportAgent == null ||
                !await _userManager.IsInRoleAsync(
                    supportAgent,
                    "SupportAgent"))
            {
                TempData["ErrorMessage"] =
                    "The selected user is not a valid support agent.";

                return RedirectToAction(nameof(AllTickets));
            }

            ticket.AssignedToUserId = supportAgent.Id;

            if (ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.Assigned;
            }

            TempData["SuccessMessage"] =
                $"Ticket #{ticket.Id} was assigned to {supportAgent.Email}.";
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(AllTickets));
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var rows = new List<AdminUserRowViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            rows.Add(new AdminUserRowViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "Unknown",
                CurrentRole = roles.FirstOrDefault() ?? "No Role",
                IsProtectedAdmin =
                    string.Equals(
                        user.Email,
                        "admin@issueforge.local",
                        StringComparison.OrdinalIgnoreCase)
            });
        }

        var viewModel = new AdminUsersViewModel
        {
            Users = rows
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(
        string userId,
        string role)
    {
        var allowedRoles = new[]
        {
            "User",
            "SupportAgent",
            "Admin"
        };

        if (!allowedRoles.Contains(role))
        {
            TempData["ErrorMessage"] = "Invalid role selected.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (string.Equals(
            user.Email,
            "admin@issueforge.local",
            StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] =
                "The primary administrator role cannot be changed.";

            return RedirectToAction(nameof(Users));
        }

        var existingRoles =
            await _userManager.GetRolesAsync(user);

        if (existingRoles.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    existingRoles);

            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Unable to remove the user's existing role.";

                return RedirectToAction(nameof(Users));
            }
        }

        var addResult =
            await _userManager.AddToRoleAsync(user, role);

        if (!addResult.Succeeded)
        {
            TempData["ErrorMessage"] =
                "Unable to assign the selected role.";

            return RedirectToAction(nameof(Users));
        }

        TempData["SuccessMessage"] =
            $"{user.Email} is now assigned to the {role} role.";

        return RedirectToAction(nameof(Users));
    }
}