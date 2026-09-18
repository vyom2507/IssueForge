using System.ComponentModel.DataAnnotations;

namespace IssueForge.Models;

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketStatus
{
    Open,
    Assigned,
    InProgress,
    Resolved,
    Closed
}

public class Ticket
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? AssignedToUserId { get; set; }
}