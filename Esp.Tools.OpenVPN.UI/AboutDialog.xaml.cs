using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.UI
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            Button_Click(sender, null);
        }
    }
}
