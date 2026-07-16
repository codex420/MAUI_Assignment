using CommunityToolkit.Mvvm.ComponentModel;
using MAUI_Assignment.Services;
using MAUI_Assignment.ViewModels;
using MAUI_Assignment.Views;
using Microsoft.Extensions.Logging;

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

        // ViewModels
        builder.Services.AddSingleton<DashboardViewModel>();

        // Views
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
