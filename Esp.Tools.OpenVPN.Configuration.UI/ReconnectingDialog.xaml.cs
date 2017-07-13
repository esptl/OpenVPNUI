using System.Windows;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    ///     Interaction logic for ReconnectingDialog.xaml
    /// </summary>
    public partial class ReconnectingDialog : DropShadowWindow
    {
        public ReconnectingDialog()
        {
            InitializeComponent();
            AllGlass = true;
        }

        private void ExitButtonClick(object pSender, RoutedEventArgs pE)
        {
            Application.Current.Shutdown();
        }
    }
}