//  This file is part of OpenVPN UI.
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

using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using Esp.Tools.OpenVPN.Hosting.Config;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages.Contracts;

namespace Esp.Tools.OpenVPN.Hosting.PipeServers
{
    public class ControllerPipeServer : BasePipeServer
    {
        private readonly OpenVPNConfigurations _configurations;


        public ControllerPipeServer(OpenVPNConfigurations pConfigurations) : base(
            Configuration.Configuration.Current.ControlPipe, 1)
        {
            _configurations = pConfigurations;
            _configurations.OutputRecieved += (pConfig, pLine) => Task.WaitAll(SendOutputRecievedMessage(pConfig, pLine));
            _configurations.StatusChanged += (pConfig,pStatus) => Task.WaitAll(SendStatusChangedMessage(pConfig, pStatus));
            _configurations.InterfaceChanged += (pConfig, pInterface) => Task.WaitAll(SendInterfaceChangedMessage(pConfig, pInterface));
            _configurations.AuthInfoRequired += (pConfig)=> Task.WaitAll(SendRequestAuthInfoMessage(pConfig));
            
        }

        #region Overrides

        protected override async Task OnConnection()
        {
            await SendConnectionInfoForAllConnections();
        }

        protected override IEnumerable<PipeAccessRule> PipeAccessRules => new[]
        {
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                PipeAccessRights.ReadWrite |
                PipeAccessRights.CreateNewInstance, AccessControlType.Allow),
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow),
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow)
        };

        protected override IEnumerable<IMessageReader> MessageReaders => new IMessageReader[]
        {
            new MessageReader<ConnectionStartInfo>("Start")
                {MessageRecieved = OnConnectionStartCommand},
            new MessageReader<ConnectionStopInfo>("Stop")
                {MessageRecieved = OnConnectionStopCommand},
            new MessageReader<ConnectionInfoCommandInfo>("Info")
                {MessageRecieved = OnConnectionInfoCommand},

            new MessageReader<ClearLogInfo>("ClearLog")
                {MessageRecieved = OnClearLogCommand},
            new MessageReader<AuthInfo>("AuthInfo")
            {
                MessageRecieved = OnSendPasswordCommand
            }
        };

        #endregion

        #region Message Sending Methods

        private async Task SendInterfaceChangedMessage(OpenVPNConfiguration pConfig, string pInterface)
        {
            await SendConnectionInfoMessage(pConfig);
        }

        private async Task SendStatusChangedMessage(OpenVPNConfiguration pConfig, ConnectionStatus pStatus)
        {
            await SendConnectionInfoMessage(pConfig);
        }

        private async Task SendOutputRecievedMessage(OpenVPNConfiguration pConfig, OutputLine pLine)
        {
            await SendMessageAsync(new ConnectionOutputMessage {Connection = pConfig.Index, Data = pLine});
        }

        private async Task SendRequestAuthInfoMessage(OpenVPNConfiguration pConfig)
        {
            await SendMessageAsync(new RequestAuthInfoMessage {Connection = pConfig.Index});
        }

        private async Task SendConnectionInfoMessage(OpenVPNConfiguration pConfig)
        {
            await SendMessageAsync(new ConnectionInfoMessage(pConfig.Index)
                {Data = pConfig.ConfigurationInfo});
        }

        private async Task SendConnectionInfoForAllConnections()
        {
            foreach (var con in _configurations)
                await SendConnectionInfoMessage(con);
        }

        #endregion

        #region Command Handlers

        private async Task OnConnectionInfoCommand(BaseMessage<ConnectionInfoCommandInfo> pInfo)
        {
            var con = _configurations[pInfo.Connection];
            await SendConnectionInfoMessage(con);
            if (pInfo.Data.Long)
                foreach (var line in con.Output)
                    await SendOutputRecievedMessage(con, line);
        }

        private async Task OnConnectionStopCommand(BaseMessage<ConnectionStopInfo> pInfo)
        {
            _configurations[pInfo.Connection].Disconnect(pInfo.Data.Kill);
        }

        private async Task OnConnectionStartCommand(BaseMessage<ConnectionStartInfo> pInfo)
        {
            _configurations[pInfo.Connection].Connect();
        }

        private async Task OnClearLogCommand(BaseMessage<ClearLogInfo> pInfo)
        {
            _configurations[pInfo.Connection].ClearOutput();
        }


        private async Task OnSendPasswordCommand(BaseMessage<AuthInfo> pInfo)
        {
            _configurations[pInfo.Connection].SendAuthInfo(pInfo);
        }

        #endregion
    }
}