namespace MAUI_Assignment.Models;

public class NavItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Url { get; set; } = string.Empty;
}
