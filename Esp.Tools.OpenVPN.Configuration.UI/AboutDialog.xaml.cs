using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : DropShadowWindow
    {
        public AboutDialog()
        {
            InitializeComponent();
            GlassMargin = new Margins(0,0,30,0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
       
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            Button_Click(sender, null);
        }
    }
}
