namespace MAUI_Assignment.Components;

public partial class TrafficView : ContentView
{
    public IDrawable DonutDrawable { get; } = new DonutDrawable();

    public TrafficView()
    {
        InitializeComponent();
    }
}

/// <summary>Simple 3-segment donut chart.</summary>
public class DonutDrawable : IDrawable
{
    private static readonly (float value, string color)[] Segments =
    {
        (34f, "#3F8CFF"),
        (55f, "#F26522"),
        (11f, "#FFC93C"),
    };

    public void Draw(ICanvas canvas, RectF rect)
    {
        float size = Math.Min(rect.Width, rect.Height) - 12;
        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        float outer = size / 2f;
        float thickness = outer * 0.36f;
        float radius = outer - thickness / 2f;

        canvas.StrokeSize = thickness;
        canvas.StrokeLineCap = LineCap.Butt;

        float start = -90f;
        foreach (var (value, color) in Segments)
        {
            float sweep = value / 100f * 360f;
            canvas.StrokeColor = Color.FromArgb(color);
            // DrawArc uses degrees; clockwise = false draws counter-clockwise,
            // so we invert start/end to sweep clockwise.
            float end = start + sweep;
            canvas.DrawArc(
                cx - radius, cy - radius,
                radius * 2f, radius * 2f,
                -start, -end,
                clockwise: true,
                closed: false);
            start = end;
        }
    }
}
