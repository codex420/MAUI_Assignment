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

    [ObservableProperty]
    private string _currentUrl = string.Empty;

    [ObservableProperty]
    private bool _isDashboardActive = true;

    [ObservableProperty]
    private string _pageHeaderTitle = "Dashboard";

    [ObservableProperty]
    private string _activeChartTab = "Daily";

    // ---------- Responsive state ----------

    /// <summary>Current page width in device-independent units.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWide))]
    [NotifyPropertyChangedFor(nameof(IsMedium))]
    [NotifyPropertyChangedFor(nameof(IsCompact))]
    [NotifyPropertyChangedFor(nameof(StatCardSpan))]
    [NotifyPropertyChangedFor(nameof(StatCardBasis))]
    [NotifyPropertyChangedFor(nameof(SummaryStatSpan))]
    [NotifyPropertyChangedFor(nameof(SummaryStatBasis))]
    [NotifyPropertyChangedFor(nameof(TopRowVertical))]
    [NotifyPropertyChangedFor(nameof(BottomRowVertical))]
    [NotifyPropertyChangedFor(nameof(SidebarIsOverlay))]
    [NotifyPropertyChangedFor(nameof(ContentPadding))]
    [NotifyPropertyChangedFor(nameof(ScrimVisible))]
    [NotifyPropertyChangedFor(nameof(DockedSidebarVisible))]
    [NotifyPropertyChangedFor(nameof(HeaderTopHeight))]
    [NotifyPropertyChangedFor(nameof(HeaderBottomHeight))]
    [NotifyPropertyChangedFor(nameof(HeaderRow))]
    [NotifyPropertyChangedFor(nameof(HeaderColumn))]
    [NotifyPropertyChangedFor(nameof(HeaderColumnSpan))]
    [NotifyPropertyChangedFor(nameof(TrafficRow))]
    [NotifyPropertyChangedFor(nameof(TrafficColumn))]
    [NotifyPropertyChangedFor(nameof(TrafficColumnSpan))]
    [NotifyPropertyChangedFor(nameof(ActivitiesRow))]
    [NotifyPropertyChangedFor(nameof(ActivitiesColumn))]
    [NotifyPropertyChangedFor(nameof(ActivitiesColumnSpan))]
    [NotifyPropertyChangedFor(nameof(OrdersRow))]
    [NotifyPropertyChangedFor(nameof(OrdersColumn))]
    [NotifyPropertyChangedFor(nameof(OrdersColumnSpan))]
    private double _pageWidth = 1280;

    /// <summary>Whether the sidebar is currently shown.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScrimVisible))]
    [NotifyPropertyChangedFor(nameof(DockedSidebarVisible))]
    private bool _isSidebarOpen = true;

    // Breakpoints
    private const double WideBreakpoint = 1100;
    private const double MediumBreakpoint = 700;

    public bool IsWide => PageWidth >= WideBreakpoint;
    public bool IsMedium => PageWidth >= MediumBreakpoint && PageWidth < WideBreakpoint;
    public bool IsCompact => PageWidth < MediumBreakpoint;

    /// <summary>On compact screens the sidebar floats over the content instead of pushing it.</summary>
    public bool SidebarIsOverlay => IsCompact;

    /// <summary>The dimming scrim is only shown when an overlay sidebar is open.</summary>
    public bool ScrimVisible => SidebarIsOverlay && IsSidebarOpen;

    /// <summary>Docked sidebar shows only when open AND not in overlay mode.</summary>
    public bool DockedSidebarVisible => IsSidebarOpen && !SidebarIsOverlay;

    /// <summary>Number of columns for the stat-card grid.</summary>
    public int StatCardSpan => IsWide ? 4 : (IsMedium ? 2 : 1);

    /// <summary>
    /// Relative flex-basis for each stat card so the row reflows 4 → 2 → 1
    /// across breakpoints. Slightly under the exact fraction to leave room
    /// for the inter-card margin.
    /// </summary>
    public Microsoft.Maui.Layouts.FlexBasis StatCardBasis
    {
        get
        {
            double fraction = IsWide ? 0.235 : (IsMedium ? 0.48 : 1.0);
            return new Microsoft.Maui.Layouts.FlexBasis((float)fraction, isRelative: true);
        }
    }

    /// <summary>Columns for the header summary-stats strip (4 wide → 2 small).</summary>
    public int SummaryStatSpan => PageWidth >= 900 ? 4 : (IsCompact ? 1 : 2);

    /// <summary>Relative flex-basis for each header summary stat (4 / 2 / 1 up).</summary>
    public Microsoft.Maui.Layouts.FlexBasis SummaryStatBasis
    {
        get
        {
            double fraction = PageWidth >= 900 ? 0.25 : (IsCompact ? 1.0 : 0.5);
            return new Microsoft.Maui.Layouts.FlexBasis((float)fraction, isRelative: true);
        }
    }

    /// <summary>Stack the Header/Traffic row vertically on small screens.</summary>
    public bool TopRowVertical => !IsWide;

    /// <summary>Stack the Activities/Orders row vertically below the wide breakpoint.</summary>
    public bool BottomRowVertical => IsCompact;

    // ---------- HeaderView responsive height settings to avoid clipping ----------
    public double HeaderTopHeight => IsCompact ? 410 : 210;
    public double HeaderBottomHeight => IsWide ? 70 : (IsMedium ? 140 : 280);

    /// <summary>Tighter padding on phones.</summary>
    public Thickness ContentPadding => IsCompact ? new Thickness(12) : new Thickness(20);

    // Row 1 grid positioning (Header + Traffic)
    public int HeaderRow => 0;
    public int HeaderColumn => 0;
    public int HeaderColumnSpan => IsWide ? 1 : 2;

    public int TrafficRow => IsWide ? 0 : 1;
    public int TrafficColumn => IsWide ? 1 : 0;
    public int TrafficColumnSpan => IsWide ? 1 : 2;

    // Row 3 grid positioning (Activities + Orders)
    public int ActivitiesRow => 0;
    public int ActivitiesColumn => 0;
    public int ActivitiesColumnSpan => IsCompact ? 2 : 1;

    public int OrdersRow => IsCompact ? 1 : 0;
    public int OrdersColumn => IsCompact ? 0 : 1;
    public int OrdersColumnSpan => IsCompact ? 2 : 1;

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

    /// <summary>
    /// Called by the page whenever its size changes. Recomputes the
    /// breakpoint and auto-opens/closes the sidebar for the new form factor.
    /// </summary>
    public void UpdateForWidth(double width)
    {
        if (width <= 0 || Math.Abs(width - PageWidth) < 0.5)
            return;

        bool wasCompact = IsCompact;
        PageWidth = width;

        // When crossing into a compact layout, hide the sidebar so content
        // gets the full width. When growing back to a docked layout, show it.
        if (IsCompact && !wasCompact)
            IsSidebarOpen = false;
        else if (!IsCompact)
            IsSidebarOpen = true;
    }

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;

    [RelayCommand]
    private void CloseSidebar() => IsSidebarOpen = false;

    [RelayCommand]
    private void SelectNav(NavItem item)
    {
        if (item is null) return;
        foreach (var n in NavItems)
            n.IsActive = ReferenceEquals(n, item);

        IsDashboardActive = item.Title == "Dashboard";
        CurrentUrl = item.Url;
        PageHeaderTitle = item.Title;

        // Re-emit so the UI re-evaluates IsActive-bound visuals.
        var snapshot = NavItems.ToList();
        NavItems.Clear();
        foreach (var n in snapshot)
            NavItems.Add(n);

        // On phones, selecting an item should dismiss the overlay menu.
        if (SidebarIsOverlay)
            IsSidebarOpen = false;
    }

    [RelayCommand]
    private void LastMonthSummary()
    {
        // Placeholder command — wired to the header button.
    }

    [RelayCommand]
    private void ChangeChartTab(string tab)
    {
        ActiveChartTab = tab;
    }
}
