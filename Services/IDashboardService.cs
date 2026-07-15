using MAUI_Assignment.Models;

namespace MAUI_Assignment.Services;

/// <summary>
/// Abstraction over the dashboard data source.
/// Today it returns hardcoded collections; in a real app the same
/// interface would be backed by an HTTP/API implementation, so the
/// ViewModels never need to change.
/// </summary>
public interface IDashboardService
{
    IReadOnlyList<NavItem> GetNavItems();
    IReadOnlyList<SummaryStat> GetSummaryStats();
    IReadOnlyList<StatCard> GetStatCards();
    IReadOnlyList<ActivityItem> GetRecentActivities();
    IReadOnlyList<OrderItem> GetOrders();
    IReadOnlyList<TrafficSlice> GetTraffic();
    (string earnings, string sales) GetHeaderTotals();
}
