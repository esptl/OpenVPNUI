﻿//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - August 2011
//
//
//  Foobar is free software: you can redistribute it and/or modify
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
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Esp.Tools.OpenVPN.Hosting.Config;
using Esp.Tools.OpenVPN.Hosting.PipeServers;
using Esp.Tools.OpenVPN.Hosting.PowerEvents;

namespace Esp.Tools.OpenVPN.ServiceHost
{
    public partial class OpenVPNHostService : ServiceBase
    {
        private OpenVPNConfigurations _configs;
        private ControllerPipeServer _controllerPipeServer;
        private PowerEventHandlers _power;
        private ConfigurationPipeServer _configPipe;

        public OpenVPNHostService()
        {
            InitializeComponent();
            CanHandlePowerEvent = true;
        }

        

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
           switch(powerStatus)
           {
               case PowerBroadcastStatus.Suspend: 
                   _power.Suspend();
                   break;
               case PowerBroadcastStatus.ResumeSuspend: 
                   _power.Resume();
                   break;
           }
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnStart(string[] pArgs)
        {

            _configs = new OpenVPNConfigurations();
           _power = new PowerEventHandlers(_configs);
           _configPipe = new ConfigurationPipeServer(_configs);
            _controllerPipeServer = new ControllerPipeServer(_configs);
          
        }

        protected override void OnStop()
        {
            _power.Suspend();
            _configPipe.Shutdown();
            _controllerPipeServer.Shutdown();
            base.OnStop();
        }

  
    }
}