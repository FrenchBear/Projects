// AboutDialog
// First attempt of an About window for an UWP app, even though it's not standard
//
// 2018-09-29   PV
// 2020-11-11   PV      nullable enable

using System;
using System.Reflection;
using System.Threading.Tasks;
using UniDataNS;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace UniSearchNS;

public sealed partial class AboutDialog: ContentDialog
{
    public AboutDialog()
    {
        InitializeComponent();

        // Main app info
        (string Title, string Description, string Version, string Copyright) = GetAppVersion(Assembly.GetExecutingAssembly());
        AssemblyTitle.Text = Title;
        AssemblyDescription.Text = Description;
        AssemblyVersion.Text = "Version " + Version;
        AssemblyCopyright.Text = Copyright;

        // UniData DLL info
        (string DataTitle, string DataDescription, string DataVersion, string DataCopyright) = GetAppVersion(typeof(UniData).GetTypeInfo().Assembly);
        UniDataTitle.Text = DataTitle;
        UniDataDescription.Text = DataDescription;
        UniDataVersion.Text = "Version " + DataVersion;
        UniDataCopyright.Text = DataCopyright;
    }

    public static (string Title, string Description, string Version, string Copyright) GetAppVersion(Assembly myAssembly)
    {
        if (myAssembly == null)
            throw new NullReferenceException();

        var aTitleAttr = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
        var aDescAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
        var aCopyrightAttr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
        //AssemblyProductAttribute aProductAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

        string version = myAssembly.GetName().Version.Major.ToString() + "." + myAssembly.GetName().Version.Minor.ToString() + "." + myAssembly.GetName().Version.Build.ToString();
        return (aTitleAttr.Title, aDescAttr.Description, version, aCopyrightAttr.Copyright);
    }

    public static async Task ShowAbout()
    {
        var w = new AboutDialog();
        await w.ShowAsync();
    }
}
