using System.Windows;
using System.Windows.Controls;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    ///     Interaction logic for CertificateView.xaml
    /// </summary>
    public partial class CertificateView : UserControl
    {
        public CertificateView()
        {
            InitializeComponent();
        }

        private void CloseDeletePopup(object pSender, RoutedEventArgs pE)
        {
            _deletePopup.IsOpen = false;
        }

        private void DeleteButtonClicked(object pSender, RoutedEventArgs pE)
        {
            _deletePopup.IsOpen = true;
        }
    }
}