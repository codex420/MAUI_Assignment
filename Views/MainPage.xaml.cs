using MAUI_Assignment.ViewModels;

namespace MAUI_Assignment.Views;

public partial class MainPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public MainPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Recompute the responsive breakpoint whenever the page is resized
        // (window resize on desktop, rotation on mobile).
        SizeChanged += OnPageSizeChanged;
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width > 0)
            _viewModel.UpdateForWidth(Width);
    }
}
