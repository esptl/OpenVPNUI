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


        public ControllerPipeServer(OpenVPNConfigurations pConfigurations) : base(Configuration.Configuration.Current.ControlPipe,1)
        {
            _configurations = pConfigurations;
            _configurations.OutputRecieved += SendOutputRecievedMessage;
            _configurations.StatusChanged += SendStatusChangedMessage;
            _configurations.InterfaceChanged += SendInterfaceChangedMessage;
            _configurations.AuthInfoRequired += SendRequestAuthInfoMessage;
        }

        #region Overrides
        protected override void OnConnection()
        {
            SendConnectionInfoForAllConnections();
        }

        protected override IEnumerable<PipeAccessRule> PipeAccessRules
        {
            get
            {
                return new[]
                           {
                               new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                                                  PipeAccessRights.ReadWrite |
                                                  PipeAccessRights.CreateNewInstance, AccessControlType.Allow),
                               new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null), PipeAccessRights.FullControl,
                                                  AccessControlType.Allow),
                               new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl,
                                                  AccessControlType.Allow)
                           };
            }
        }

        protected override IEnumerable<IMessageReader> MessageReaders
        {
            get
            {
                return new IMessageReader[]
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
            }
        }
        #endregion

        #region Message Sending Methods

        private void SendInterfaceChangedMessage(OpenVPNConfiguration pConfig, string pInterface)
        {
            SendConnectionInfoMessage(pConfig);
            
        }

        private void SendStatusChangedMessage(OpenVPNConfiguration pConfig, ConnectionStatus pStatus)
        {
            SendConnectionInfoMessage(pConfig);
            
        }

        private void SendOutputRecievedMessage(OpenVPNConfiguration pConfig, OutputLine pLine)
        {
            SendMessage(new ConnectionOutputMessage {Connection = pConfig.Index, Data = pLine});
        }

        private void SendRequestAuthInfoMessage(OpenVPNConfiguration pConfig)
        {
            SendMessage(new RequestAuthInfoMessage { Connection = pConfig.Index});
            
        }

        private void SendConnectionInfoMessage(OpenVPNConfiguration pConfig)
        {
            SendMessage(new ConnectionInfoMessage(pConfig.Index)
                                                      {Data = pConfig.ConfigurationInfo});
        }

        private void SendConnectionInfoForAllConnections()
        {
            foreach (OpenVPNConfiguration con in _configurations)
                SendConnectionInfoMessage(con);
        }

        #endregion

        #region Command Handlers
        private void OnConnectionInfoCommand(BaseMessage<ConnectionInfoCommandInfo> pInfo)
        {
            OpenVPNConfiguration con = _configurations[pInfo.Connection];
            SendConnectionInfoMessage(con);
            if (pInfo.Data.Long)
            {
                foreach (OutputLine line in con.Output)
                {
                    SendOutputRecievedMessage(con, line);
                }
            }
        }

        private void OnConnectionStopCommand(BaseMessage<ConnectionStopInfo> pInfo)
        {
            _configurations[pInfo.Connection].Disconnect(pInfo.Data.Kill);
        }

        private void OnConnectionStartCommand(BaseMessage<ConnectionStartInfo> pInfo)
        {
            _configurations[pInfo.Connection].Connect();
        }

        private void OnClearLogCommand(BaseMessage<ClearLogInfo> pInfo)
        {
            _configurations[pInfo.Connection].ClearOutput();
        }


        private void OnSendPasswordCommand(BaseMessage<AuthInfo> pInfo)
        {
            _configurations[pInfo.Connection].SendAuthInfo(pInfo);
        }
        #endregion
    }
}