using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUI_Assignment.Models;
using MAUI_Assignment.Services;

namespace MAUI_Assignment.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _service;
    private readonly IPdfService? _pdfService;
    private readonly List<OrderItem> _allOrders = new();
    private const int PageSize = 5;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayStart))]
    [NotifyPropertyChangedFor(nameof(DisplayEnd))]
    [NotifyPropertyChangedFor(nameof(TotalEntries))]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(PaginationStatusText))]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    [NotifyPropertyChangedFor(nameof(CanGoPrevious))]
    [NotifyPropertyChangedFor(nameof(PageIndicatorText))]
    private int _currentPage = 1;

    private static Color Res(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var v) == true && v is Color c
            ? c
            : Colors.Gray;

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

    [ObservableProperty]
    private bool _isAddOrderModalVisible = false;

    [ObservableProperty]
    private string _newOrderCustomer = string.Empty;

    [ObservableProperty]
    private string _newOrderFrom = string.Empty;

    [ObservableProperty]
    private string _newOrderPrice = string.Empty;

    [ObservableProperty]
    private string _newOrderStatus = "Process";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    [NotifyPropertyChangedFor(nameof(ModalTitle))]
    private OrderItem? _editingOrder;

    [ObservableProperty]
    private bool _isInfoModalVisible = false;

    [ObservableProperty]
    private string _infoModalMessage = string.Empty;

    [ObservableProperty]
    private bool _isSummaryModalVisible = false;

    [ObservableProperty]
    private string _summaryModalDetails = string.Empty;

    public bool IsEditing => EditingOrder != null;
    public string ModalTitle => IsEditing ? $"Edit Order #{EditingOrder!.Invoice}" : "Add New Order";

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

#if WINDOWS
            // On Windows, a half-width card in the medium range is too narrow for the
            // Bounce Rate card's fixed 124px label column + line chart, so the chart
            // gets squashed. Force one card per row until the window is wide enough
            // that four-across (wide) kicks in.
            if (!IsWide)
                fraction = 1.0;
#endif

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
            double fraction = PageWidth >= 900 ? 0.24 : (IsCompact ? 1.0 : 0.49);
            return new Microsoft.Maui.Layouts.FlexBasis((float)fraction, isRelative: true);
        }
    }

    /// <summary>Stack the Header/Traffic row vertically on small screens.</summary>
    public bool TopRowVertical => !IsWide;

    /// <summary>Stack the Activities/Orders row vertically below the wide breakpoint.</summary>
    public bool BottomRowVertical => IsCompact;

    // ---------- HeaderView responsive height settings to avoid clipping ----------
    public double HeaderTopHeight => IsCompact ? 330 : 165;
    public double HeaderBottomHeight => IsWide ? 58 : (IsMedium ? 116 : 232);

    /// <summary>Tighter padding on phones.</summary>
    public Thickness ContentPadding => IsCompact ? new Thickness(12, 60, 12, 12) : new Thickness(16);

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

    public int TotalEntries => _allOrders.Count;
    public int TotalPages => (int)Math.Ceiling((double)TotalEntries / PageSize);
    public int DisplayStart => TotalEntries == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayEnd => Math.Min(CurrentPage * PageSize, TotalEntries);
    public string PaginationStatusText => $"Showing {DisplayStart} to {DisplayEnd} of {TotalEntries} entries";
    public string PageIndicatorText => $"Page {CurrentPage} of {TotalPages}";

    public bool CanGoNext => CurrentPage < TotalPages;
    public bool CanGoPrevious => CurrentPage > 1;

    public ObservableCollection<NavItem> NavItems { get; } = new();
    public ObservableCollection<SummaryStat> SummaryStats { get; } = new();
    public ObservableCollection<StatCard> StatCards { get; } = new();
    public ObservableCollection<ActivityItem> RecentActivities { get; } = new();
    public ObservableCollection<OrderItem> Orders { get; } = new();
    public ObservableCollection<TrafficSlice> Traffic { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    public DashboardViewModel(IDashboardService service, IPdfService? pdfService = null)
    {
        _service = service;
        _pdfService = pdfService;
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
        _allOrders.Clear();
        _allOrders.AddRange(_service.GetOrders());
        UpdatePagedOrders();
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
        SummaryModalDetails = "Last Month Performance Summary:\n\n" +
                              "• Total Earnings: $24,890 (+12% growth)\n" +
                              "• Total Completed Orders: 1,420\n" +
                              "• Top Customer Location: Brazil (450 orders)\n" +
                              "• Active Platform Users: 890\n" +
                              "• Resolution Rate: 98.4%\n" +
                              "• Average Delivery Time: 2.3 days";
        IsSummaryModalVisible = true;
    }

    [RelayCommand]
    private void CloseSummaryModal()
    {
        IsSummaryModalVisible = false;
    }

    [RelayCommand]
    private void ChangeChartTab(string tab)
    {
        ActiveChartTab = tab;
    }

    partial void OnCurrentPageChanged(int value)
    {
        UpdatePagedOrders();
    }

    partial void OnSearchQueryChanged(string value)
    {
        CurrentPage = 1;
        UpdatePagedOrders();
    }

    private List<OrderItem> GetFilteredOrders()
    {
        var query = (SearchQuery ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(query))
            return _allOrders;

        return _allOrders.Where(o =>
            (o.Invoice != null && o.Invoice.ToLowerInvariant().Contains(query)) ||
            (o.Customer != null && o.Customer.ToLowerInvariant().Contains(query)) ||
            (o.From != null && o.From.ToLowerInvariant().Contains(query)) ||
            (o.Status != null && o.Status.ToLowerInvariant().Contains(query)) ||
            (o.Price != null && o.Price.ToLowerInvariant().Contains(query))
        ).ToList();
    }

    private void UpdatePagedOrders()
    {
        Orders.Clear();
        var filteredList = GetFilteredOrders();
        int startIndex = (CurrentPage - 1) * PageSize;
        var pagedList = filteredList.Skip(startIndex).Take(PageSize);
        foreach (var order in pagedList)
        {
            Orders.Add(order);
        }

        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++)
        {
            PageNumbers.Add(i);
        }

        OnPropertyChanged(nameof(DisplayStart));
        OnPropertyChanged(nameof(DisplayEnd));
        OnPropertyChanged(nameof(TotalEntries));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PaginationStatusText));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(PageIndicatorText));
    }

    [RelayCommand]
    private void ChangePage(object pageParam)
    {
        if (pageParam == null) return;
        int page = 0;
        if (pageParam is int intVal)
        {
            page = intVal;
        }
        else if (pageParam is long longVal)
        {
            page = (int)longVal;
        }
        else if (pageParam is string strVal && int.TryParse(strVal, out int parsed))
        {
            page = parsed;
        }
        else
        {
            try
            {
                page = Convert.ToInt32(pageParam);
            }
            catch {}
        }

        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CanGoNext)
            CurrentPage++;
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CanGoPrevious)
            CurrentPage--;
    }

    [RelayCommand]
    private void ShowAddOrderModal()
    {
        EditingOrder = null;
        NewOrderCustomer = string.Empty;
        NewOrderFrom = string.Empty;
        NewOrderPrice = string.Empty;
        NewOrderStatus = "Process";
        IsAddOrderModalVisible = true;
    }

    [RelayCommand]
    private void CloseAddOrderModal()
    {
        IsAddOrderModalVisible = false;
    }

    [RelayCommand]
    private void EditOrder(OrderItem order)
    {
        if (order is null) return;
        EditingOrder = order;
        NewOrderCustomer = order.Customer;
        NewOrderFrom = order.From;
        NewOrderPrice = order.Price.Replace("$", string.Empty);
        NewOrderStatus = order.Status;
        IsAddOrderModalVisible = true;
    }

    [RelayCommand]
    private void ShowInfo()
    {
        InfoModalMessage = "These are the order statuses of all the people inside the dashboard.";
        IsInfoModalVisible = true;
    }

    // ---------- Row selection + delete ----------

    /// <summary>True when at least one order row is currently selected.</summary>
    public bool HasSelection => _allOrders.Any(o => o.IsSelected);

    // Debounce so that a single physical click, which can raise both the
    // PointerPressed and Tap gestures on the same row, toggles selection once
    // rather than twice (which would visually do nothing).
    private OrderItem? _lastToggled;
    private DateTime _lastToggledAt;

    /// <summary>Toggle the selected state of a row (tap to select / deselect).</summary>
    [RelayCommand]
    private void ToggleOrderSelection(OrderItem order)
    {
        if (order is null) return;

        var now = DateTime.UtcNow;
        if (ReferenceEquals(order, _lastToggled) &&
            (now - _lastToggledAt) < TimeSpan.FromMilliseconds(250))
            return;

        _lastToggled = order;
        _lastToggledAt = now;

        order.IsSelected = !order.IsSelected;
        OnPropertyChanged(nameof(HasSelection));
    }

    /// <summary>
    /// Delete every selected row after a confirmation prompt. If nothing is
    /// selected, informs the user instead of doing nothing silently.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedOrders()
    {
        var selected = _allOrders.Where(o => o.IsSelected).ToList();
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;

        if (selected.Count == 0)
        {
            if (page is not null)
                await page.DisplayAlert("Delete Orders", "Select one or more rows first, then tap delete.", "OK");
            return;
        }

        bool confirm = page is null || await page.DisplayAlert(
            "Delete Orders",
            $"Delete {selected.Count} selected order{(selected.Count > 1 ? "s" : "")}?",
            "Delete", "Cancel");

        if (!confirm) return;

        foreach (var order in selected)
            _allOrders.Remove(order);

        // Keep the current page valid after removals.
        if (CurrentPage > TotalPages)
            CurrentPage = Math.Max(1, TotalPages);

        UpdatePagedOrders();
        OnPropertyChanged(nameof(HasSelection));
    }

    /// <summary>
    /// Generate a PDF of the orders and save it directly to the Downloads folder
    /// on both Android and Windows.
    /// </summary>
    [RelayCommand]
    private async Task PrintOrders()
    {
        var source = GetFilteredOrders();
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;

        if (_pdfService is null)
        {
            if (page is not null)
                await page.DisplayAlert("Export PDF", "PDF export isn't available on this platform.", "OK");
            return;
        }

        string? savedPath = await _pdfService.ExportOrdersAsync(source);

        if (page is not null)
        {
            if (savedPath is not null)
                await page.DisplayAlert("PDF Downloaded", $"Saved to:\n{savedPath}", "OK");
            else
                await page.DisplayAlert("Export PDF", "Sorry, the PDF could not be saved.", "OK");
        }
    }

    [RelayCommand]
    private void CloseInfoModal()
    {
        IsInfoModalVisible = false;
    }

    [RelayCommand]
    private void SubmitNewOrder()
    {
        string customer = string.IsNullOrWhiteSpace(NewOrderCustomer) ? "Guest" : NewOrderCustomer.Trim();
        string from = string.IsNullOrWhiteSpace(NewOrderFrom) ? "USA" : NewOrderFrom.Trim();
        
        string price = (NewOrderPrice ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(price))
        {
            price = "$0";
        }
        else if (!price.StartsWith("$"))
        {
            price = "$" + price;
        }

        string status = NewOrderStatus ?? "Process";
        string colorKey = status switch
        {
            "Open" => "StatusOpen",
            "On Hold" => "StatusHold",
            _ => "StatusProcess"
        };

        if (IsEditing && EditingOrder != null)
        {
            EditingOrder.Customer = customer;
            EditingOrder.From = from;
            EditingOrder.Price = price;
            EditingOrder.Status = status;
            EditingOrder.StatusColor = Res(colorKey);
            EditingOrder = null;
        }
        else
        {
            int nextInvoice = _allOrders.Count > 0 
                ? _allOrders.Max(o => int.TryParse(o.Invoice, out var val) ? val : 0) + 1 
                : 12411;

            var newOrder = new OrderItem
            {
                Invoice = nextInvoice.ToString(),
                Customer = customer,
                From = from,
                Price = price,
                Status = status,
                StatusColor = Res(colorKey)
            };

            _allOrders.Insert(0, newOrder);
        }

        IsAddOrderModalVisible = false;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchQuery = string.Empty;
        }
        else
        {
            if (CurrentPage == 1)
            {
                UpdatePagedOrders();
            }
            else
            {
                CurrentPage = 1;
            }
        }
    }
}
