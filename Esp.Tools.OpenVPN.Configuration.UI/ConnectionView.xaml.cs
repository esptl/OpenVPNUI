using System.Windows;
using System.Windows.Controls;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    ///     Interaction logic for ConnectionView.xaml
    /// </summary>
    public partial class ConnectionView : UserControl
    {
        public ConnectionView()
        {
            InitializeComponent();
        }

        private void DeleteButtonClicked(object pSender, RoutedEventArgs pE)
        {
            _deletePopup.IsOpen = true;
        }

        private void CloseDeletePopup(object pSender, RoutedEventArgs pE)
        {
            _deletePopup.IsOpen = false;
        }

        private void SettingsButtonClicked(object pSender, RoutedEventArgs pE)
        {
            _settingsPopup.IsOpen = true;
        }
    }
}