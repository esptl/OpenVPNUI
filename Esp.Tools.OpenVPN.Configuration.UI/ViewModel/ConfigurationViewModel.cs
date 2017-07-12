//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - September 2011
//
//
//  OpenVPN UI is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  OpenVPN UI is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with OpenVPN UI.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Windows;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly IViewModelDialogs _dialogs;
        private ConfigurationPipeClient _configClient;

        public ConfigurationViewModel(IViewModelDialogs pDialogs)
        {
            _dialogs = pDialogs;
            try
            {
                _configClient = new ConfigurationPipeClient();
               
                _configClient.Disconnected += () => Dispatch(() => pDialogs.ShowReconnecting(_configClient));

                ControllerAccess = new GroupAccessViewModel(pDialogs,Configuration.Current.ControllerAccess);

                Certificates = new CertificatesViewModel(pDialogs,_configClient);
                Connections = new ConnectionsViewModel(pDialogs, _configClient);
                General = new GeneralViewModel(pDialogs, _configClient);

                CloseCommand = new BasicCommand(() => Application.Current.Shutdown());
                AboutCommand = new BasicCommand(pDialogs.ShowAbout);
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public GeneralViewModel General
        {
            get; private set; }

        public BasicCommand AboutCommand
        {
            get; 
            private set; 
        }

        public void ShowReconnectingIfRequired()
        {
            if(!_configClient.IsConnected)
                _dialogs.ShowReconnecting(_configClient);
            
        }

        public GroupAccessViewModel ControllerAccess { get; private set; }
        public CertificatesViewModel Certificates { get; private set; }
        public ConnectionsViewModel Connections { get; private set; }

        public BasicCommand CloseCommand { get; private set; }
    }
}
