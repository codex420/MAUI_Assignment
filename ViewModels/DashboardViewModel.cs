using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUI_Assignment.Models;
using MAUI_Assignment.Services;

namespace MAUI_Assignment.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _service;

    [ObservableProperty]
    private string _currentMonthEarnings = string.Empty;

    [ObservableProperty]
    private string _currentMonthSales = string.Empty;

    public ObservableCollection<NavItem> NavItems { get; } = new();
    public ObservableCollection<SummaryStat> SummaryStats { get; } = new();
    public ObservableCollection<StatCard> StatCards { get; } = new();
    public ObservableCollection<ActivityItem> RecentActivities { get; } = new();
    public ObservableCollection<OrderItem> Orders { get; } = new();
    public ObservableCollection<TrafficSlice> Traffic { get; } = new();

    public DashboardViewModel(IDashboardService service)
    {
        _service = service;
        Load();
    }

    private void Load()
    {
        var (earnings, sales) = _service.GetHeaderTotals();
        CurrentMonthEarnings = earnings;
        CurrentMonthSales = sales;

        Fill(NavItems, _service.GetNavItems());
        Fill(SummaryStats, _service.GetSummaryStats());
        Fill(StatCards, _service.GetStatCards());
        Fill(RecentActivities, _service.GetRecentActivities());
        Fill(Orders, _service.GetOrders());
        Fill(Traffic, _service.GetTraffic());
    }

    private static void Fill<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }

    [RelayCommand]
    private void SelectNav(NavItem item)
    {
        if (item is null) return;
        foreach (var n in NavItems)
            n.IsActive = ReferenceEquals(n, item);
        // Re-emit so the UI re-evaluates IsActive-bound visuals.
        var snapshot = NavItems.ToList();
        NavItems.Clear();
        foreach (var n in snapshot)
            NavItems.Add(n);
    }

    [RelayCommand]
    private void LastMonthSummary()
    {
        // Placeholder command — wired to the header button.
    }
}
