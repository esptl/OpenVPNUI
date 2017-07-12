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
using Esp.Tools.OpenVPN.UI.Model;

namespace Esp.Tools.OpenVPN.UI
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl
    {
        public LogViewer()
        {
            InitializeComponent();
        }

       
        private void TextBox_TextChanged(object pSender, TextChangedEventArgs pE)
        {
            (pSender as TextBox).ScrollToEnd();
        }

        private void DoubleAnimation_Changed(object sender, EventArgs e)
        {
            
        }
   }
}
