using Esp.Tools.OpenVPN.Configuration.UI.ViewModel;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    ///     Interaction logic for RestartServiceView.xaml
    /// </summary>
    public partial class RestartServiceView
    {
        public RestartServiceView()
        {
            InitializeComponent();
            AllGlass = true;
            var restartServiceViewModel = new RestartServiceViewModel(null);
            restartServiceViewModel.Completed += Close;
            DataContext = restartServiceViewModel;
        }
    }
}