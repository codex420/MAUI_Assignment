using Microsoft.Maui.Graphics;
using MAUI_Assignment.Models;

namespace MAUI_Assignment.Components;

public partial class CardView : ContentView
{
    private MiniChartDrawable? _lineDrawable;

    public CardView()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is StatCard card)
        {
            AreaChart.Drawable = new MiniChartDrawable(MiniChartKind.Area, card.Accent);
            _lineDrawable = new MiniChartDrawable(MiniChartKind.Line, card.Accent)
            {
                LineProfileIndex = RangePicker.SelectedIndex < 0 ? 0 : RangePicker.SelectedIndex
            };
            LineChart.Drawable = _lineDrawable;
            AreaChart.Invalidate();
            LineChart.Invalidate();
        }
    }

    private void OnRangeChanged(object? sender, EventArgs e)
    {
        if (_lineDrawable is null || sender is not Picker picker) return;
        _lineDrawable.LineProfileIndex = picker.SelectedIndex < 0 ? 0 : picker.SelectedIndex;
        LineChart.Invalidate();
    }
}

public enum MiniChartKind
{
    Area,
    Line
}

/// <summary>
/// Small chart used inside the stat cards. Draws a smooth filled area (Page View
/// style) or a wavy line with markers (Bounce Rate style). Sample points are fixed
/// so every render produces the same silhouette as the design reference.
/// </summary>
public class MiniChartDrawable : IDrawable
{
    // Rising, choppy profile for the "Page View" area card (climbs toward the right).
    private static readonly float[] AreaValues = { 0.28f, 0.18f, 0.52f, 0.40f, 0.70f, 0.55f, 0.88f, 0.72f, 0.80f };

    // Distinct line profiles per Bounce Rate range so switching the dropdown visibly
    // changes the wave. Index maps to the Picker order: Monthly / Weekly / Daily.
    private static readonly float[][] LineProfiles =
    {
        new[] { 0.30f, 0.72f, 0.40f, 0.20f, 0.55f, 0.82f, 0.60f, 0.30f, 0.48f }, // Monthly
        new[] { 0.50f, 0.35f, 0.65f, 0.45f, 0.28f, 0.60f, 0.40f, 0.72f, 0.55f }, // Weekly
        new[] { 0.20f, 0.55f, 0.30f, 0.78f, 0.45f, 0.25f, 0.68f, 0.42f, 0.62f }, // Daily
    };

    private readonly MiniChartKind _kind;
    private readonly Color _color;

    /// <summary>Selected Bounce Rate profile (0 = Monthly). Ignored for area charts.</summary>
    public int LineProfileIndex { get; set; }

    public MiniChartDrawable(MiniChartKind kind, Color color)
    {
        _kind = kind;
        _color = color;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (_kind == MiniChartKind.Area)
            DrawArea(canvas, rect);
        else
            DrawLine(canvas, rect);
    }

    private void DrawArea(ICanvas canvas, RectF rect)
    {
        var pts = BuildPoints(rect, AreaValues);
        float bottom = rect.Bottom;

        var fill = SmoothPath(pts, bottom, closePath: true);
        canvas.FillColor = _color.WithAlpha(0.28f);
        canvas.FillPath(fill);

        var line = SmoothPath(pts, bottom, closePath: false);
        canvas.StrokeColor = _color;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(line);
    }

    private void DrawLine(ICanvas canvas, RectF rect)
    {
        int idx = Math.Clamp(LineProfileIndex, 0, LineProfiles.Length - 1);
        var pts = BuildPoints(rect, LineProfiles[idx]);

        var line = SmoothPath(pts, rect.Bottom, closePath: false);
        canvas.StrokeColor = _color;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(line);

        // Data-point markers along the wave.
        canvas.FillColor = _color;
        foreach (var p in pts)
            canvas.FillCircle(p.X, p.Y, 2.6f);
    }

    private static PointF[] BuildPoints(RectF rect, float[] values)
    {
        float left = rect.Left + 4;
        float right = rect.Right - 4;
        float top = rect.Top + 6;
        float bottom = rect.Bottom - 4;
        float width = right - left;
        float height = bottom - top;
        float step = width / (values.Length - 1);

        var pts = new PointF[values.Length];
        for (int i = 0; i < values.Length; i++)
            pts[i] = new PointF(left + step * i, bottom - values[i] * height);
        return pts;
    }

    private static PathF SmoothPath(PointF[] points, float bottom, bool closePath)
    {
        var path = new PathF();
        int length = points.Length;
        if (length < 2) return path;

        if (closePath)
        {
            path.MoveTo(points[0].X, bottom);
            path.LineTo(points[0].X, points[0].Y);
        }
        else
        {
            path.MoveTo(points[0].X, points[0].Y);
        }

        const float k = 0.2f;
        for (int i = 0; i < length - 1; i++)
        {
            var p0 = points[Math.Max(i - 1, 0)];
            var p1 = points[i];
            var p2 = points[i + 1];
            var p3 = points[Math.Min(i + 2, length - 1)];

            float cp1X = p1.X + (p2.X - p0.X) * k;
            float cp1Y = p1.Y + (p2.Y - p0.Y) * k;
            float cp2X = p2.X - (p3.X - p1.X) * k;
            float cp2Y = p2.Y - (p3.Y - p1.Y) * k;

            path.CurveTo(cp1X, cp1Y, cp2X, cp2Y, p2.X, p2.Y);
        }

        if (closePath)
        {
            path.LineTo(points[length - 1].X, bottom);
            path.Close();
        }

        return path;
    }
}
