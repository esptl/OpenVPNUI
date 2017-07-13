using System.Windows.Controls;

namespace Esp.Tools.OpenVPN.UI
{
    /// <summary>
    ///     Interaction logic for ErrorView.xaml
    /// </summary>
    public partial class ErrorView : UserControl
    {
        public ErrorView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object pSender, TextChangedEventArgs pE)
        {
            (pSender as TextBox).ScrollToEnd();
        }
    }
}