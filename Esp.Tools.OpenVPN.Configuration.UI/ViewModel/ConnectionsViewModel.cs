//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - October 2011
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class ConnectionsViewModel : ViewModelBase
    {
        private readonly ConfigurationPipeClient _configClient;
        private readonly IViewModelDialogs _dialogs;

        private IEnumerable<ConnectionViewModel> _connections;

        public ConnectionsViewModel(IViewModelDialogs pDialogs, ConfigurationPipeClient pConfigClient)
        {
            _dialogs = pDialogs;
            _configClient = pConfigClient;
            if (_configClient.Configurations != null)
                UpdateConfigurations();
            _configClient.ConfigurationsChanged += OnConfigurationsChanged;
            ImportCommand = new BasicCommand(OnImport);
        }

        public BasicCommand ImportCommand { get; }

        public IEnumerable<ConnectionViewModel> Connections
        {
            get => _connections;
            set
            {
                _connections = value;
                OnPropertyChanged("Connections");
            }
        }

        private void OnImport()
        {
            var con = _dialogs.GetConnectionFile();
            if (con == null) return;
            _configClient.SendInstallConfigurationCommand(con);
        }

        private void OnConfigurationsChanged()
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(UpdateConfigurations));
        }

        private void UpdateConfigurations()
        {
            Connections = _configClient.Configurations.Select(pX => new ConnectionViewModel(_configClient, pX));
        }
    }
}