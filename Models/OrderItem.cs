namespace MAUI_Assignment.Models;

public class OrderItem
{
    public string Invoice { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Color StatusColor { get; set; } = Colors.Gray;
}
