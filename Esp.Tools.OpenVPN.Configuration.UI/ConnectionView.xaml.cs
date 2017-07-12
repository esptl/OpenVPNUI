using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    /// Interaction logic for ConnectionView.xaml
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
