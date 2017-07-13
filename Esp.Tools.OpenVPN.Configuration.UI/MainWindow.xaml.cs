using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Input;
using Esp.Tools.OpenVPN.Configuration.UI.ViewModel;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DropShadowWindow
    {
        private readonly ConfigurationViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                if (TapDeviceManager.GetTapDevices().ToArray().Length == 0)
                    TapDeviceManager.SetupTapDevice();
            }
            catch (Win32Exception we)
            {
                TapDeviceManager.SetupTapDevice();
            }
            var dialogs = new ViewModelDialogs(this);
            _viewModel = new ConfigurationViewModel(dialogs);

            DataContext = _viewModel;

            GlassMargin = new Margins(12, 12, 105, 12);
        }


        private void Grid_MouseDown(object pSender, MouseButtonEventArgs pE)
        {
            DragMove();
        }

        private void DropShadowWindow_SourceInitialized(object pSender, EventArgs pE)
        {
            _viewModel.ShowReconnectingIfRequired();
        }
    }
}