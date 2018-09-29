// AboutDialog
// First attempt of an About window for an UWP app, even though it's not standard
//
// 2018-09-29   PV


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UniDataNS;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace UniSearchUWPNS
{
    public sealed partial class AboutDialog : ContentDialog
    {
        public AboutDialog()
        {
            this.InitializeComponent();

            // Main app info
            (string Title, string Description, string Version, string Copyright) = GetAppVersion();
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


        public static (string Title, string Description, string Version, string Copyright) GetAppVersion(Assembly myAssembly = null)
        {
            if (myAssembly == null)
                myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            AssemblyTitleAttribute aTitleAttr = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
            AssemblyDescriptionAttribute aDescAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
            AssemblyCopyrightAttribute aCopyrightAttr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
            //AssemblyProductAttribute aProductAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

            string version = myAssembly.GetName().Version.Major.ToString() + "." + myAssembly.GetName().Version.Minor.ToString() + "." + myAssembly.GetName().Version.Build.ToString();
            return (aTitleAttr.Title, aDescAttr.Description, version, aCopyrightAttr.Copyright);
        }


        public async static Task ShowAbout()
        {
            var w = new AboutDialog();
            await w.ShowAsync();
        }
    }
}
