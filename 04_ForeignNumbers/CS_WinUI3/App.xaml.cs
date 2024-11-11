using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForeignNumbersWinUI3;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App: Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() => InitializeComponent();

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // TODO This code defaults the app to a single instance app. If you need multi instance app, remove this part.
        // Read: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle#single-instancing-in-applicationonlaunched
        // If this is the first instance launched, then register it as the "main" instance.
        // If this isn't the first instance launched, then "main" will already be registered,
        // so retrieve it.
        var mainInstance = AppInstance.FindOrRegisterForKey("main");
        var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

        // If the instance that's executing the OnLaunched handler right now
        // isn't the "main" instance.
        if (!mainInstance.IsCurrent)
        {
            // Redirect the activation (and args) to the "main" instance, and exit.
            await mainInstance.RedirectActivationToAsync(activatedEventArgs);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }

        // TODO This code handles app activation types. Add any other activation kinds you want to handle.
        // Read: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle#file-type-association
        //if (activatedEventArgs.Kind == ExtendedActivationKind.File)
        //{
        //    OnFileActivated(activatedEventArgs);
        //}

        // Initialize MainWindow here
        Window = new MainWindow();
        Window.Activate();
        WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(Window);
    }

    public static MainWindow Window { get; private set; }

    public static IntPtr WindowHandle { get; private set; }
}
