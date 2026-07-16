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
                ChartView?.Invalidate();
                ChartViewCompact?.Invalidate();
            }
        }
    }
}

/// <summary>
/// Lightweight two-series smoothed area chart used as the header visual.
/// Values are static sample points; a real app would feed these from data.
/// </summary>
public class LineChartDrawable : IDrawable
{
    private static readonly float[] DailyStore  = { 0.30f, 0.42f, 0.28f, 0.55f, 0.40f, 0.62f, 0.35f, 0.90f, 0.55f, 0.48f };
    private static readonly float[] DailyOnline = { 0.20f, 0.30f, 0.22f, 0.35f, 0.30f, 0.40f, 0.28f, 0.52f, 0.38f, 0.34f };

    private static readonly float[] WeeklyStore  = { 0.50f, 0.35f, 0.60f, 0.42f, 0.70f, 0.55f, 0.80f, 0.62f, 0.75f, 0.90f };
    private static readonly float[] WeeklyOnline = { 0.30f, 0.25f, 0.45f, 0.32f, 0.50f, 0.40f, 0.60f, 0.48f, 0.55f, 0.70f };

    private static readonly float[] MonthlyStore  = { 0.40f, 0.55f, 0.48f, 0.70f, 0.58f, 0.80f, 0.65f, 0.85f, 0.72f, 0.95f };
    private static readonly float[] MonthlyOnline = { 0.25f, 0.38f, 0.34f, 0.52f, 0.42f, 0.60f, 0.48f, 0.68f, 0.54f, 0.75f };

    private static readonly float[] YearlyStore  = { 0.60f, 0.72f, 0.58f, 0.85f, 0.70f, 0.92f, 0.78f, 0.98f, 0.85f, 0.99f };
    private static readonly float[] YearlyOnline = { 0.40f, 0.50f, 0.42f, 0.65f, 0.52f, 0.75f, 0.60f, 0.82f, 0.68f, 0.80f };

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

    public void Draw(ICanvas canvas, RectF rect)
    {
        DrawSeries(canvas, rect, _currentStore,  Color.FromArgb("#F26522"));
        DrawSeries(canvas, rect, _currentOnline, Color.FromArgb("#4FC3F7"));
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

        // Filled area using smooth path
        var fill = CreateSmoothPath(values.Length, P, bottom, closePath: true);
        canvas.FillColor = color.WithAlpha(0.14f);
        canvas.FillPath(fill);

        // Stroke line using smooth path
        var line = CreateSmoothPath(values.Length, P, bottom, closePath: false);
        canvas.StrokeColor = color;
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
