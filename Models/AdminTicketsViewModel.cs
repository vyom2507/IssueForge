namespace IssueForge.Models;

public class AdminTicketsViewModel
{
    public List<AdminTicketRowViewModel> Tickets { get; set; } = new();

    public List<SupportAgentOptionViewModel> SupportAgents { get; set; } = new();
}

public class AdminTicketRowViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public string CreatedByEmail { get; set; } = "Unknown";

    public string? AssignedToUserId { get; set; }

    public string? AssignedToEmail { get; set; }
}

public class SupportAgentOptionViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}