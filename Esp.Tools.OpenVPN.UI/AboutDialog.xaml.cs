using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Navigation;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.UI
{
    /// <summary>
    ///     Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : DropShadowWindow
    {
        public AboutDialog()
        {
            InitializeComponent();
            GlassMargin = new Margins(0, 0, 30, 0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FormFadeOut.Completed += (pSender, pArgs) => Close();
            FormFadeOut.Begin();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            Button_Click(sender, null);
        }
    }
}