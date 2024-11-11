using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace ForeignNumbersWinUI3;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow: Window
{
    readonly AppWindow m_AppWindow;

    public MainWindow()
    {
        InitializeComponent();
        m_AppWindow = GetAppWindowForCurrentWindow();

        // No customization of title bar, use standard one
        m_AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 450, Height = 250 });
        m_AppWindow.Title = "ForeignNumbers WinUI3";
        m_AppWindow.SetIcon(@"Assets\Icons\ForeignNumbers.ico");

        MainGrid.Loaded += (s, e) => PageFrame.Navigate(typeof(MainPage));
    }

    private AppWindow GetAppWindowForCurrentWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }
}
