namespace MAUI_Assignment.Components;

/// <summary>
/// Platform host for the Order Status card. Android and Windows use completely
/// separate implementations: Android keeps the gesture-recognizer rows
/// (<see cref="OrderStatusViewAndroid"/>), while Windows uses a Button-based
/// click target (<see cref="OrderStatusViewWindows"/>) because WinUI does not
/// reliably deliver taps to a Grid's gesture recognizers inside a ScrollViewer.
/// This host simply swaps in the right one at construction time; both share the
/// same DashboardViewModel via the inherited BindingContext.
/// </summary>
public class OrderStatusView : ContentView
{
    public OrderStatusView()
    {
#if WINDOWS
        Content = new OrderStatusViewWindows();
#else
        Content = new OrderStatusViewAndroid();
#endif
    }
}
