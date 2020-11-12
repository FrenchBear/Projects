// Standard About Window for UniSearch
// 2016-09-26   PV  First version
// 2017-01-02   PV  En français

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using UniDataNS;

#nullable enable


namespace UniSearchNS
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    internal sealed partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            // Main app info
            (string Title, string Description, string Version, string Copyright) = GetAppVersion(System.Reflection.Assembly.GetExecutingAssembly());
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

            Loaded += (s, e) => { OKButton.Focus(); };
        }

        public static (string Title, string Description, string Version, string Copyright) GetAppVersion(Assembly myAssembly)
        {
            if (myAssembly == null) throw new NullReferenceException();

            AssemblyTitleAttribute? aTitleAttr = (AssemblyTitleAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
            AssemblyDescriptionAttribute? aDescAttr = (AssemblyDescriptionAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
            AssemblyCopyrightAttribute? aCopyrightAttr = (AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
            //AssemblyProductAttribute aProductAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

            string version = myAssembly.GetName()?.Version?.Major.ToString() + "." + myAssembly.GetName()?.Version?.Minor.ToString() + "." + myAssembly.GetName()?.Version?.Build.ToString();
            return (aTitleAttr?.Title ?? string.Empty, aDescAttr?.Description ?? string.Empty, version, aCopyrightAttr?.Copyright ?? string.Empty);
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}