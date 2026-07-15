using MAUI_Assignment.ViewModels;

namespace MAUI_Assignment.Views;

public partial class MainPage : ContentPage
{
    public MainPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
