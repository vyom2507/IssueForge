namespace IssueForge.Models;

public class AdminUsersViewModel
{
    public List<AdminUserRowViewModel> Users { get; set; } = new();
}

public class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CurrentRole { get; set; } = "No Role";

    public bool IsProtectedAdmin { get; set; }
}