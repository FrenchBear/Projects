// T59v7WPF - Visual rendering of colorized/reformatted T59 source code
// It's a WPF app, but rendering is done in HTML using WebView2 component
//
// 2025-11-29   PV
// 2025-12-04   PV      MVVM model

using System.ComponentModel;
using System.IO;
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

    private void SourceTextBox_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Handled = true;
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length == 1)
            {
                var fi = new FileInfo(files[0]);
                if (fi.Extension.Equals(".src", System.StringComparison.CurrentCultureIgnoreCase) || fi.Extension.Equals(".t59", System.StringComparison.CurrentCultureIgnoreCase))
                    e.Effects = DragDropEffects.Copy;
            }
        }
    }

    private void SourceTextBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length == 1)
            {
                var fi = new FileInfo(files[0]);
                if (fi.Extension.Equals(".src", System.StringComparison.CurrentCultureIgnoreCase) || fi.Extension.Equals(".t59", System.StringComparison.CurrentCultureIgnoreCase))
                    try
                    {
                        VM.SourceCode = File.ReadAllText(files[0]);
                        VM.FileName = files[0];
                        VM.IsDirty = false;
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Erreur en lisant le fichier:\n\n{ex.Message}", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
            }
        }
    }
}
