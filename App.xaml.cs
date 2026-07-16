namespace MAUI_Assignment;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell())
        {
            Title = "MAUI Dashboard"
        };

#if WINDOWS || MACCATALYST
        const int width = 1280;
        const int height = 820;
        window.Width = width;
        window.Height = height;
#endif

        return window;
    }
}
