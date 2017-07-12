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
    /// Interaction logic for CertificateView.xaml
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
