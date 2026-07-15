namespace MAUI_Assignment.Components;

public partial class HeaderView : ContentView
{
    public IDrawable ChartDrawable { get; } = new LineChartDrawable();

    public HeaderView()
    {
        InitializeComponent();
    }
}

/// <summary>
/// Lightweight two-series smoothed area chart used as the header visual.
/// Values are static sample points; a real app would feed these from data.
/// </summary>
public class LineChartDrawable : IDrawable
{
    private static readonly float[] Store  = { 0.30f, 0.42f, 0.28f, 0.55f, 0.40f, 0.62f, 0.35f, 0.90f, 0.55f, 0.48f };
    private static readonly float[] Online = { 0.20f, 0.30f, 0.22f, 0.35f, 0.30f, 0.40f, 0.28f, 0.52f, 0.38f, 0.34f };

    public void Draw(ICanvas canvas, RectF rect)
    {
        DrawSeries(canvas, rect, Store,  Color.FromArgb("#F26522"));
        DrawSeries(canvas, rect, Online, Color.FromArgb("#4FC3F7"));
    }

    private static void DrawSeries(ICanvas canvas, RectF rect, float[] values, Color color)
    {
        if (values.Length < 2) return;

        float left = rect.Left + 4;
        float right = rect.Right - 4;
        float top = rect.Top + 8;
        float bottom = rect.Bottom - 4;
        float width = right - left;
        float height = bottom - top;
        float step = width / (values.Length - 1);

        PointF P(int i) => new(left + step * i, bottom - values[i] * height);

        // Filled area
        var fill = new PathF();
        fill.MoveTo(left, bottom);
        for (int i = 0; i < values.Length; i++)
        {
            var p = P(i);
            fill.LineTo(p.X, p.Y);
        }
        fill.LineTo(right, bottom);
        fill.Close();

        canvas.FillColor = color.WithAlpha(0.14f);
        canvas.FillPath(fill);

        // Stroke line
        var line = new PathF();
        var first = P(0);
        line.MoveTo(first.X, first.Y);
        for (int i = 1; i < values.Length; i++)
        {
            var p = P(i);
            line.LineTo(p.X, p.Y);
        }

        canvas.StrokeColor = color;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(line);
    }
}
