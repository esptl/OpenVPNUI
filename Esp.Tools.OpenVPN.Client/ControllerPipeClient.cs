//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - August 2011
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
using System.Collections;
using System.Collections.Generic;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Commands;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages.Contracts;

namespace Esp.Tools.OpenVPN.Client
{
    public class ControllerPipeClient : BasePipeClient, IEnumerable<ConfigurationInfo>
    {
        private readonly List<ConfigurationInfo> _connections = new List<ConfigurationInfo>();

        public ControllerPipeClient() : base(Configuration.Configuration.Current.ControlPipe)
        {
        }

        public int ConnectionCount => _connections.Count;

        public ConfigurationInfo this[int pConnection] => _connections[pConnection];

        protected override IEnumerable<IMessageReader> MessageReaders =>
            new IMessageReader[]
            {
                new MessageReader<ConfigurationInfo>("Info")
                    {MessageRecieved = OnConnectionInfo},
                new MessageReader<OutputLine>("Output")
                    {MessageRecieved = OnConnectionOutput},
                new MessageReader<RequestPasswordInfo>("AuthInfo")
                    {MessageRecieved = OnRequestPassword},
                new MessageReader<InitializedInfo>("Initialized")
                    {MessageRecieved = OnInitialized}
            };


        public IEnumerator<ConfigurationInfo> GetEnumerator()
        {
            return _connections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event Action<BaseMessage<RequestPasswordInfo>> RequestPassword;


        public event Action<BaseMessage<ConfigurationInfo>> ConnectionInfo;
        public event Action<BaseMessage<OutputLine>> Message;
        public event Action<BaseMessage<InitializedInfo>> Initialized;

        public void ClearLog(int pConnection)
        {
            if (_pipe.IsConnected)
                UtilityMethods.WriteCommandResult(_pipe, new ClearLogCommand {Connection = pConnection});
            else
                Reconnect();
        }

        public void GetFullInfo(int pConnection)
        {
            UtilityMethods.WriteCommandResult(_pipe,
                new ConnectionInfoCommand {Connection = pConnection, Data = {Long = true}});
        }

        public void SendUsernamePassword(int pConnection, string pUsername, string pPassword)
        {
            UtilityMethods.WriteCommandResult(_pipe,
                new SendAuthInfoCommand
                {
                    Connection = pConnection,
                    Data = {UserName = pUsername, Password = pPassword}
                });
        }

        private void OnInitialized(BaseMessage<InitializedInfo> pInfo)
        {
            Initialized?.Invoke(pInfo);
        }

        private void OnRequestPassword(BaseMessage<RequestPasswordInfo> pInfo)
        {
            RequestPassword?.Invoke(pInfo);
        }

        private void OnConnectionOutput(BaseMessage<OutputLine> pObj)
        {
            Message?.Invoke(pObj);
        }

        private void OnConnectionInfo(BaseMessage<ConfigurationInfo> pObj)
        {
            for (var i = pObj.Connection - _connections.Count + 1; i > 0; i--)
                _connections.Add(null);
            _connections[pObj.Connection] = pObj.Data;

            ConnectionInfo?.Invoke(pObj);
        }
    }
}