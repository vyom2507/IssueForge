namespace IssueForge.Models;

public class SupportAgentDashboardViewModel
{
    public int TotalAssignedTickets { get; set; }

    public int AssignedTickets { get; set; }

    public int InProgressTickets { get; set; }

    public int ResolvedTickets { get; set; }

    public int ClosedTickets { get; set; }

    public List<Ticket> Tickets { get; set; } = new();
}