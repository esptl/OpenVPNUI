using System;
using System.Windows.Controls;

namespace Esp.Tools.OpenVPN.UI
{
    /// <summary>
    ///     Interaction logic for LogViewer.xaml
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