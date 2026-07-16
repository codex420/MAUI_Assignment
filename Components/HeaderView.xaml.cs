using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MAUI_Assignment.ViewModels;

namespace MAUI_Assignment.Components;

public enum ChartTab
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public partial class HeaderView : ContentView
{
    public IDrawable ChartDrawable { get; } = new LineChartDrawable();

    public HeaderView()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is DashboardViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            UpdateChartData(vm.ActiveChartTab);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardViewModel.ActiveChartTab))
        {
            if (BindingContext is DashboardViewModel vm)
            {
                UpdateChartData(vm.ActiveChartTab);
            }
        }
    }

    private void UpdateChartData(string tabStr)
    {
        if (Enum.TryParse<ChartTab>(tabStr, out var tab))
        {
            if (ChartDrawable is LineChartDrawable lineChart)
            {
                lineChart.UpdateData(tab);
                InvalidateCharts();
            }
        }
    }

    // ---- Legend toggles: hide/show a series; state lives on the drawable so it
    //      survives Daily/Weekly/Monthly/Yearly tab switches. ----

    private void OnToggleOnline(object? sender, EventArgs e)
    {
        if (ChartDrawable is LineChartDrawable c)
        {
            c.ShowOnline = !c.ShowOnline;
            double op = c.ShowOnline ? 1.0 : 0.35;
            OnlineLegend.Opacity = op;
            OnlineLegendCompact.Opacity = op;
            InvalidateCharts();
        }
    }

    private void OnToggleStore(object? sender, EventArgs e)
    {
        if (ChartDrawable is LineChartDrawable c)
        {
            c.ShowStore = !c.ShowStore;
            double op = c.ShowStore ? 1.0 : 0.35;
            StoreLegend.Opacity = op;
            StoreLegendCompact.Opacity = op;
            InvalidateCharts();
        }
    }

    private void InvalidateCharts()
    {
        ChartView?.Invalidate();
        ChartViewCompact?.Invalidate();
    }
}

/// <summary>
/// Lightweight two-series smoothed area chart used as the header visual.
/// Values are static sample points; a real app would feed these from data.
/// </summary>
public class LineChartDrawable : IDrawable
{
    // Richer, more complex sample series (more points + several peaks/dips) so the
    // curve reads like the multi-wave graph in the design reference.
    private static readonly float[] DailyStore  = { 0.32f, 0.28f, 0.46f, 0.34f, 0.58f, 0.40f, 0.30f, 0.52f, 0.42f, 0.66f, 0.48f, 0.38f, 0.90f, 0.60f, 0.50f };
    private static readonly float[] DailyOnline = { 0.20f, 0.18f, 0.30f, 0.24f, 0.38f, 0.28f, 0.22f, 0.36f, 0.30f, 0.44f, 0.34f, 0.28f, 0.60f, 0.42f, 0.36f };

    private static readonly float[] WeeklyStore  = { 0.44f, 0.36f, 0.58f, 0.40f, 0.68f, 0.50f, 0.38f, 0.62f, 0.48f, 0.78f, 0.56f, 0.44f, 0.85f, 0.66f, 0.72f };
    private static readonly float[] WeeklyOnline = { 0.28f, 0.24f, 0.40f, 0.28f, 0.48f, 0.36f, 0.26f, 0.44f, 0.34f, 0.56f, 0.40f, 0.32f, 0.62f, 0.48f, 0.52f };

    private static readonly float[] MonthlyStore  = { 0.40f, 0.52f, 0.44f, 0.64f, 0.50f, 0.74f, 0.56f, 0.46f, 0.68f, 0.58f, 0.82f, 0.62f, 0.92f, 0.70f, 0.78f };
    private static readonly float[] MonthlyOnline = { 0.26f, 0.36f, 0.30f, 0.46f, 0.36f, 0.54f, 0.40f, 0.32f, 0.50f, 0.42f, 0.60f, 0.46f, 0.68f, 0.52f, 0.58f };

    private static readonly float[] YearlyStore  = { 0.56f, 0.66f, 0.54f, 0.78f, 0.62f, 0.86f, 0.68f, 0.58f, 0.82f, 0.70f, 0.94f, 0.74f, 0.99f, 0.82f, 0.88f };
    private static readonly float[] YearlyOnline = { 0.38f, 0.46f, 0.38f, 0.58f, 0.46f, 0.66f, 0.52f, 0.44f, 0.62f, 0.52f, 0.72f, 0.56f, 0.80f, 0.64f, 0.70f };

    private float[] _currentStore = DailyStore;
    private float[] _currentOnline = DailyOnline;

    public void UpdateData(ChartTab tab)
    {
        switch (tab)
        {
            case ChartTab.Daily:
                _currentStore = DailyStore;
                _currentOnline = DailyOnline;
                break;
            case ChartTab.Weekly:
                _currentStore = WeeklyStore;
                _currentOnline = WeeklyOnline;
                break;
            case ChartTab.Monthly:
                _currentStore = MonthlyStore;
                _currentOnline = MonthlyOnline;
                break;
            case ChartTab.Yearly:
                _currentStore = YearlyStore;
                _currentOnline = YearlyOnline;
                break;
        }
    }

    private static readonly Color StoreColor  = Color.FromArgb("#F26522");
    private static readonly Color OnlineColor = Color.FromArgb("#4FC3F7");
    private static readonly Color GridColor    = Color.FromArgb("#ECEEF3");
    private static readonly Color GridTextColor = Color.FromArgb("#9AA1AC");

    // Left gutter reserved for the Y-axis scale labels, bottom gutter for the plot.
    private const float AxisGutter = 30f;
    private const int HorizontalLines = 5; // grid rows (=> HorizontalLines gridlines)

    /// <summary>Series visibility toggled by the legend; persists across tab switches.</summary>
    public bool ShowOnline { get; set; } = true;
    public bool ShowStore { get; set; } = true;

    public void Draw(ICanvas canvas, RectF rect)
    {
        float left = rect.Left + AxisGutter;
        float right = rect.Right - 6;
        float top = rect.Top + 10;
        float bottom = rect.Bottom - 6;

        DrawGrid(canvas, left, top, right, bottom);

        // Blue (Online) sits behind with a neutral gray area fill, matching the design.
        if (ShowOnline)
            DrawSeries(canvas, left, top, right, bottom, _currentOnline, OnlineColor,
                       Color.FromArgb("#E4E6EB"), 0.75f);
        // Orange (Store) sits on top with a soft orange fill.
        if (ShowStore)
            DrawSeries(canvas, left, top, right, bottom, _currentStore, StoreColor,
                       StoreColor, 0.14f);
    }

    private static void DrawGrid(ICanvas canvas, float left, float top, float right, float bottom)
    {
        float width = right - left;
        float height = bottom - top;

        canvas.FontSize = 9;
        canvas.StrokeSize = 1;

        // Horizontal grid lines + Y scale labels (0 .. 100).
        for (int i = 0; i <= HorizontalLines; i++)
        {
            float y = top + height * i / HorizontalLines;
            canvas.StrokeColor = GridColor;
            canvas.DrawLine(left, y, right, y);

            int scaleValue = 100 - (100 * i / HorizontalLines);
            canvas.FontColor = GridTextColor;
            canvas.DrawString(scaleValue.ToString(), left - AxisGutter, y - 6,
                              AxisGutter - 6, 12, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // Vertical grid lines (X scale divisions).
        const int verticalLines = 8;
        for (int i = 0; i <= verticalLines; i++)
        {
            float x = left + width * i / verticalLines;
            canvas.StrokeColor = GridColor;
            canvas.DrawLine(x, top, x, bottom);
        }
    }

    private static void DrawSeries(ICanvas canvas, float left, float top, float right, float bottom,
                                   float[] values, Color strokeColor, Color fillColor, float fillAlpha)
    {
        if (values.Length < 2) return;

        float width = right - left;
        float height = bottom - top;
        float step = width / (values.Length - 1);

        PointF P(int i) => new(left + step * i, bottom - values[i] * height);

        // Filled area using smooth path
        var fill = CreateSmoothPath(values.Length, P, bottom, closePath: true);
        canvas.FillColor = fillColor.WithAlpha(fillAlpha);
        canvas.FillPath(fill);

        // Stroke line using smooth path
        var line = CreateSmoothPath(values.Length, P, bottom, closePath: false);
        canvas.StrokeColor = strokeColor;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(line);
    }

    private static PathF CreateSmoothPath(int length, Func<int, PointF> getPoint, float bottom, bool closePath)
    {
        var path = new PathF();
        if (length < 2) return path;

        var points = new PointF[length];
        for (int i = 0; i < length; i++)
            points[i] = getPoint(i);

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
