using System.Windows.Input;
using MAUI_Assignment.ViewModels;

namespace MAUI_Assignment.Components;

public partial class SidebarView : ContentView
{
    public SidebarView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Exposes the ViewModel's nav-selection command as a strongly-typed
    /// property so the item template can bind to it without a
    /// loosely-typed BindingContext hop.
    /// </summary>
    public ICommand? SelectCommand =>
        (BindingContext as DashboardViewModel)?.SelectNavCommand;
}
