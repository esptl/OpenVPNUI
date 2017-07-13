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
using System.Collections.Generic;
using Esp.Tools.OpenVPN.Hosting.Config;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;

namespace Esp.Tools.OpenVPN.Hosting.PowerEvents
{
    public class PowerEventHandlers
    {
        private readonly OpenVPNConfigurations _configurations;
        private readonly List<OpenVPNConfiguration> _connectionsToRestart = new List<OpenVPNConfiguration>();

        public PowerEventHandlers(OpenVPNConfigurations pConfigurations)
        {
            _configurations = pConfigurations;
        }

        public void Suspend()
        {
            _connectionsToRestart.Clear();
            foreach (var connection in _configurations)
                if (connection.Status != ConnectionStatus.Disconnected &&
                    connection.Status != ConnectionStatus.Disconnecting)
                {
                    connection.Disconnect(false);
                    _connectionsToRestart.Add(connection);
                }
        }

        public void Resume()
        {
            foreach (var connection in _connectionsToRestart)
                if (connection.Status == ConnectionStatus.Disconnected)
                {
                    connection.Connect();
                }
                else
                {
                    var con = connection;
                    Action<ConnectionStatus> reconnect = null;
                    reconnect = pStatus =>
                    {
                        if (pStatus != ConnectionStatus.Disconnected)
                            return;
                        con.StatusChanged -= reconnect;
                        con.Connect();
                    };
                    connection.StatusChanged += reconnect;
                }
            _connectionsToRestart.Clear();
        }
    }
}