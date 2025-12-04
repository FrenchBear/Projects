// T59v7WPF - Visual rendering of colorized/reformatted T59 source code
// It's a WPF app, but rendering is done in HTML using WebView2 component
//
// 2025-11-29   PV
// 2025-12-04   PV      MVVM model

using System.ComponentModel;
using System.Windows;

namespace T59v7WPF;

public partial class MainWindow: Window
{
    internal const string AppName = "T59v7WPF";

    private readonly ViewModel VM;

    public MainWindow()
    {
        InitializeComponent();

        Model m = new();
        VM = new ViewModel(m, this);
        m.SetViewModel(VM);
        DataContext = VM;
    }
    internal void UpdateTitle()
    {
        string msg = VM.IsDirty ? "*" : "";
        if (VM.FileName != null)
            msg += VM.FileName;
        if (msg != "")
            msg += " – ";
        msg += AppName;
        Title = msg;
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        if (VM.IsDirty)
            e.Cancel = !VM.CanContinue();
        else
            base.OnClosing(e);
    }

    internal async void DisplayColorizedHtml(string value)
    {
        await webView1.EnsureCoreWebView2Async();
        webView1.NavigateToString(value);
    }

    internal async void DisplayReformattedHtml(string value)
    {
        await webView2.EnsureCoreWebView2Async();
        webView2.NavigateToString(value);
    }
}
