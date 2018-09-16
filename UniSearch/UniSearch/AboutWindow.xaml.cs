// Standard About Window for UniSearch
// 2016-09-26   PV  First version
// 2017-01-02   PV  En français

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace UniSearchNS
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    internal partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            AssemblyTitleAttribute aTitleAttr = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
            AssemblyDescriptionAttribute aDescAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
            string sAssemblyVersion = myAssembly.GetName().Version.ToString();
            AssemblyCopyrightAttribute aCopyrightAttr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));

            AssemblyTitle.Text = aTitleAttr.Title;
            AssemblyDescription.Text = aDescAttr.Description;
            AssemblyVersion.Text = "Version " + sAssemblyVersion;
            AssemblyCopyright.Text = aCopyrightAttr.Copyright;

            Loaded += (s, e) => { OKButton.Focus(); };
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}