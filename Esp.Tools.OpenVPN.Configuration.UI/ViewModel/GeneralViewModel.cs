using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class GeneralViewModel : ViewModelBase
    {
        private IEnumerable<string> _tapDevices;

        public GeneralViewModel(IViewModelDialogs pDialogs, ConfigurationPipeClient pConfigClient)
        {
            var thread = new Thread(() =>
            {
                var interfaces = Configuration.Current.TapInterfaces.ToArray();
                Dispatch(() => TapDevices = interfaces.Select(pX => pX.Name));
            });
            thread.Priority = ThreadPriority.BelowNormal;

            thread.Start();
        }

        public IEnumerable<string> TapDevices
        {
            get => _tapDevices;
            set
            {
                _tapDevices = value;
                OnPropertyChanged("TapDevices");
            }
        }
    }
}