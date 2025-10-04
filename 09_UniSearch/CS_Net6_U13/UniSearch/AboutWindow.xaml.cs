// Standard About Window for UniSearch
// 2016-09-26   PV  First version
// 2017-01-02   PV  En français

using System;
using System.Reflection;
using System.Windows;
using UniDataNS;

#nullable enable

namespace UniSearch;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
internal sealed partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();

        // Main app info
        (string title, string description, string version, string copyright) = GetAppVersion(Assembly.GetExecutingAssembly());
        AssemblyTitle.Text = title;
        AssemblyDescription.Text = description;
        AssemblyVersion.Text = "Version " + version;
        AssemblyCopyright.Text = copyright;

        // UniData DLL info
        (string dataTitle, string dataDescription, string dataVersion, string dataCopyright) = GetAppVersion(typeof(UniData).GetTypeInfo().Assembly);
        UniDataTitle.Text = dataTitle;
        UniDataDescription.Text = dataDescription;
        UniDataVersion.Text = "Version " + dataVersion;
        UniDataCopyright.Text = dataCopyright;

        Loaded += (s, e) => OkButton.Focus();
    }

    public static (string title, string description, string version, string copyright) GetAppVersion(Assembly myAssembly)
    {
        if (myAssembly == null) throw new NullReferenceException();

        var aTitleAttr = (AssemblyTitleAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
        var aDescAttr = (AssemblyDescriptionAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
        var aCopyrightAttr = (AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
        //AssemblyProductAttribute aProductAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

        string version = myAssembly.GetName().Version?.Major + "." + myAssembly.GetName().Version?.Minor + "." + myAssembly.GetName().Version?.Build;
        return (aTitleAttr?.Title ?? string.Empty, aDescAttr?.Description ?? string.Empty, version, aCopyrightAttr?.Copyright ?? string.Empty);
    }

    private void OKButton_Click(object sender, RoutedEventArgs e) 
        => Close();
}