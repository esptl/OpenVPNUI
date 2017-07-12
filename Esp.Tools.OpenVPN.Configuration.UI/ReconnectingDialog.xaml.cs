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
using System.Windows.Shapes;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    /// Interaction logic for ReconnectingDialog.xaml
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
