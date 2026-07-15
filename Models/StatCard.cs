namespace MAUI_Assignment.Models;

public class StatCard
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public Color Background { get; set; } = Colors.White;
    public Color Accent { get; set; } = Colors.Gray;
    public string Glyph { get; set; } = ""; // chart-ish placeholder
    public StatCardKind Kind { get; set; } = StatCardKind.Bars;
}

public enum StatCardKind
{
    Bars,
    Area,
    Line
}
