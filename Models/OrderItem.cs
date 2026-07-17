using CommunityToolkit.Mvvm.ComponentModel;

namespace MAUI_Assignment.Models;

public partial class OrderItem : ObservableObject
{
    public string Invoice { get; set; } = string.Empty;

    [ObservableProperty]
    private string _customer = string.Empty;

    [ObservableProperty]
    private string _from = string.Empty;

    [ObservableProperty]
    private string _price = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private Color _statusColor = Colors.Gray;

    /// <summary>True when the user has tapped this row for multi-select delete.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RowBackground))]
    private bool _isSelected;

    /// <summary>Highlight colour for the row when selected; white otherwise.</summary>
    public Color RowBackground => IsSelected ? Color.FromArgb("#E8F1FF") : Colors.White;
}
