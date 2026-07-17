using CommunityToolkit.Mvvm.ComponentModel;
using MAUI_Assignment.Services;
using MAUI_Assignment.ViewModels;
using MAUI_Assignment.Views;
using Microsoft.Extensions.Logging;

#if WINDOWS
using WinUIBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinUIColors = Microsoft.UI.Colors;
#endif

namespace MAUI_Assignment;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("segmdl2.ttf", "SegoeMDL2");
            });

        // Services
        builder.Services.AddSingleton<IDashboardService, DashboardService>();

#if ANDROID
        builder.Services.AddSingleton<IPdfService, MAUI_Assignment.Platforms.Android.PdfService>();
#elif WINDOWS
        builder.Services.AddSingleton<IPdfService, MAUI_Assignment.Platforms.Windows.PdfService>();
#endif

        // ViewModels
        builder.Services.AddSingleton<DashboardViewModel>();

        // Views
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if WINDOWS
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("CustomPickerDropdown", (handler, view) =>
        {
            var whiteBrush = new WinUIBrush(WinUIColors.White);
            var blackBrush = new WinUIBrush(WinUIColors.Black);
            
            // Set ComboBox popup dropdown background
            handler.PlatformView.Resources["ComboBoxDropDownBackground"] = whiteBrush;
            handler.PlatformView.Resources["ComboBoxDropDownBackgroundPointerOver"] = whiteBrush;
            
            // Set ComboBox item text foreground colors
            handler.PlatformView.Resources["ComboBoxItemForeground"] = blackBrush;
            handler.PlatformView.Resources["ComboBoxItemForegroundPointerOver"] = blackBrush;
            handler.PlatformView.Resources["ComboBoxItemForegroundSelected"] = blackBrush;
            handler.PlatformView.Resources["ComboBoxItemForegroundSelectedPointerOver"] = blackBrush;

            // Set ComboBox item background hover states
            handler.PlatformView.Resources["ComboBoxItemBackgroundPointerOver"] = new WinUIBrush(Windows.UI.Color.FromArgb(255, 240, 240, 240));
            handler.PlatformView.Resources["ComboBoxItemBackgroundSelected"] = new WinUIBrush(Windows.UI.Color.FromArgb(255, 230, 230, 230));
            handler.PlatformView.Resources["ComboBoxItemBackgroundSelectedPointerOver"] = new WinUIBrush(Windows.UI.Color.FromArgb(255, 220, 220, 220));
        });
#endif

        return builder.Build();
    }
}
