// AboutWindow
// Standard About form
//
// 2024-01-03   PV
// 2026-01-20	PV		Net10 C#14

using System.Reflection;
using System.Windows;

namespace QwirkleUI;

public partial class AboutWindow: Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var myAssembly = Assembly.GetExecutingAssembly();
        var aTitleAttr = (AssemblyTitleAttribute?)System.Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
        var aDescAttr = (AssemblyDescriptionAttribute?)System.Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
        string? sAssemblyVersion = myAssembly.GetName().Version?.ToString();
        var aCopyrightAttr = (AssemblyCopyrightAttribute?)System.Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));

        AssemblyTitle.Text = aTitleAttr?.Title;
        AssemblyDescription.Text = aDescAttr?.Description;
        AssemblyVersion.Text = "Version " + sAssemblyVersion;
        AssemblyCopyright.Text = aCopyrightAttr?.Copyright;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e) => Close();
}
