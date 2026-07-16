using MAUI_Assignment.Models;

namespace MAUI_Assignment.Services;

/// <summary>
/// Static, in-memory implementation of <see cref="IDashboardService"/>.
/// Colors are resolved from the merged ResourceDictionary so the data
/// and the theme stay in sync.
/// </summary>
public class DashboardService : IDashboardService
{
    private static Color Res(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var v) == true && v is Color c
            ? c
            : Colors.Gray;

    private static readonly string[] RandomUrls = new[]
    {
        "https://dotnet.microsoft.com",
        "https://learn.microsoft.com",
        "https://www.wikipedia.org",
        "https://news.ycombinator.com",
        "https://www.w3schools.com",
        "https://example.com",
        "https://www.worldometers.info",
        "https://httpbin.org",
        "https://www.gutenberg.org",
        "https://archive.org"
    };

    public (string earnings, string sales) GetHeaderTotals() => ("$3468.96", "82");

    public IReadOnlyList<NavItem> GetNavItems()
    {
        var items = new List<NavItem>
        {
            new() { Icon = "", Title = "Dashboard", IsActive = true },
            new() { Icon = "", Title = "Widgets" },
            new() { Icon = "", Title = "UI Elements" },
            new() { Icon = "", Title = "Advanced UI" },
            new() { Icon = "", Title = "Form Elements" },
            new() { Icon = "", Title = "Editors" },
            new() { Icon = "", Title = "Charts" },
            new() { Icon = "", Title = "Tables" },
            new() { Icon = "", Title = "Popups" },
            new() { Icon = "", Title = "Notifications" },
            new() { Icon = "", Title = "Icons" },
            new() { Icon = "", Title = "Maps" },
            new() { Icon = "", Title = "User Pages" },
            new() { Icon = "", Title = "Error Pages" },
            new() { Icon = "", Title = "General Pages" },
            new() { Icon = "", Title = "E-Commerce" },
            new() { Icon = "", Title = "E-mail" },
            new() { Icon = "", Title = "Calendar" },
            new() { Icon = "", Title = "Todo List" },
            new() { Icon = "", Title = "Gallery" },
            new() { Icon = "", Title = "Documentation" },
        };

        var random = new Random();
        foreach (var item in items)
        {
            if (item.Title != "Dashboard")
            {
                item.Url = RandomUrls[random.Next(RandomUrls.Length)];
            }
        }

        return items;
    }

    public IReadOnlyList<SummaryStat> GetSummaryStats() => new List<SummaryStat>
    {
        new() { Icon = "\U0001F947", IconColor = Res("IconOrange"), Title = "Wallet Balance",  Value = "$4,567.53" },
        new() { Icon = "\U0001F48E", IconColor = Res("IconBlue"),   Title = "Referral Earning", Value = "$1689.53" },
        new() { Icon = "\U0001F4C8", IconColor = Res("IconGreen"),  Title = "Estimate Sales",   Value = "$2851.53" },
        new() { Icon = "\U0001F4CA", IconColor = Res("IconOrange"), Title = "Earning",          Value = "$52,567.53" },
    };

    public IReadOnlyList<StatCard> GetStatCards() => new List<StatCard>
    {
        new() { Title = "Revinue Status", Value = "$432", Note = "Jan 01 - Jan 10", Background = Res("CardBlue"),   Accent = Res("DonutBlue"),   Kind = StatCardKind.Bars },
        new() { Title = "Page View",      Value = "$432", Note = "",                Background = Res("CardYellow"), Accent = Res("IconYellow"),  Kind = StatCardKind.Area },
        new() { Title = "Bounce Rate",    Value = "$432", Note = "Monthly",         Background = Res("CardPeach"),  Accent = Res("Accent"),      Kind = StatCardKind.Line },
        new() { Title = "Revinue Status", Value = "$432", Note = "Jan 01 - Jan 10", Background = Res("CardPurple"), Accent = Res("IconBlue"),    Kind = StatCardKind.Bars },
    };

    public IReadOnlyList<ActivityItem> GetRecentActivities() => new List<ActivityItem>
    {
        new() { TimeAgo = "42 Mins Ago", Icon = "\U0001F4CB", IconBackground = Res("IconBlue"),   Title = "Task Updated",      Author = "Nikolai",  Detail = "Updated a Task" },
        new() { TimeAgo = "1 day Ago",   Icon = "\U0001F4B0", IconBackground = Res("IconOrange"), Title = "Deal Added",        Author = "Panchi",   Detail = "Updated a Task" },
        new() { TimeAgo = "42 Mins Ago", Icon = "\U0001F4C4", IconBackground = Res("IconTeal"),   Title = "Published Article", Author = "Rasel",    Detail = "Published an Article" },
        new() { TimeAgo = "1 day Ago",   Icon = "\U0001F517", IconBackground = Res("IconYellow"), Title = "Dock Updated",      Author = "Reshmi",   Detail = "Updated a Dock" },
        new() { TimeAgo = "1 day Ago",   Icon = "\U0001F4AC", IconBackground = Res("IconGreen"),  Title = "Replyed Comment",   Author = "Jenathon", Detail = "Added a Comment" },
    };

    public IReadOnlyList<OrderItem> GetOrders() => new List<OrderItem>
    {
        new() { Invoice = "12386", Customer = "Charly Dues",   From = "Brazil", Price = "$299",  Status = "Process", StatusColor = Res("StatusProcess") },
        new() { Invoice = "12386", Customer = "Marko",         From = "Italy",  Price = "$2642", Status = "Open",    StatusColor = Res("StatusOpen") },
        new() { Invoice = "12386", Customer = "Denyel Onak",   From = "Russia", Price = "$981",  Status = "On Hold", StatusColor = Res("StatusHold") },
        new() { Invoice = "12386", Customer = "Belgin Bastana", From = "Korea", Price = "$369",  Status = "Process", StatusColor = Res("StatusProcess") },
        new() { Invoice = "12386", Customer = "Sarti Onuska",  From = "Japan",  Price = "$1240", Status = "Open",    StatusColor = Res("StatusOpen") },
    };

    public IReadOnlyList<TrafficSlice> GetTraffic() => new List<TrafficSlice>
    {
        new() { Label = "Facebook",      Percent = "34%", Color = Res("DonutBlue") },
        new() { Label = "Youtube",       Percent = "55%", Color = Res("DonutOrange") },
        new() { Label = "Direct Search", Percent = "11%", Color = Res("DonutYellow") },
    };
}
