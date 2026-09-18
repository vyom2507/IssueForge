namespace IssueForge.Models;

public class AdminDashboardViewModel
{
    public int TotalTickets { get; set; }

    public int OpenTickets { get; set; }

    public int AssignedTickets { get; set; }

    public int InProgressTickets { get; set; }

    public int ResolvedTickets { get; set; }

    public int ClosedTickets { get; set; }

    public List<Ticket> RecentTickets { get; set; } = new();
}