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
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Commands;
using Esp.Tools.OpenVPN.IPCProtocol.Messages;

namespace Esp.Tools.OpenVPN.Client
{
    public abstract class BasePipeClient
    {
        private readonly byte[] _buffer = new byte[200000];
        private readonly string _pipeName;
        private IMessageReader[] _messageTypes;
        protected NamedPipeClientStream _pipe;

        protected BasePipeClient(string pPipeName)
        {
            _pipeName = pPipeName;
            var thread = new Thread(Reconnect);
            thread.Start();
        }

        protected abstract IEnumerable<IMessageReader> MessageReaders { get; }

        protected void SendCommand(IMessage pMessage)
        {
            if (_pipe.CanWrite)
                UtilityMethods.WriteCommandResult(_pipe,
                    pMessage);
        }


        public void Connect(int pConnection)
        {
            if (_pipe.IsConnected)
                UtilityMethods.WriteCommandResult(_pipe, new ConnectionStartCommand {Connection = pConnection});
            else
                Reconnect();
        }

        private void OnReadData(IAsyncResult pAr)
        {
            try
            {
                if (_pipe.IsAsync)
                {
                    var read = _pipe.EndRead(pAr);
                    if (read > 0)
                    {
                        var data = new byte[read];
                        Array.Copy(_buffer, 0, data, 0, read);
                        UtilityMethods.ReadMessage(data, _messageTypes);
                    }

                    _pipe.BeginRead(_buffer, 0, _buffer.Length, OnReadData, null);
                }
            }
            catch (IOException)
            {
                Reconnect();
            }
        }



        protected void Reconnect()
        {
            while (true)
            {
                try
                {
                    if (_messageTypes == null)
                    {
                        var lst = MessageReaders.ToList();
                        lst.Add(new MessageReader<ShutDownInfo>(ShutDownMessage.MessageKey)
                            { MessageRecieved = OnShutDown });
                        _messageTypes = lst.ToArray();
                    }

                    _pipe = new NamedPipeClientStream("localhost", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                    if (Connecting != null)
                        Connecting();
                    _pipe.Connect();
                    if (Connected != null)
                        Connected();
                    _pipe.BeginRead(_buffer, 0, _buffer.Length, OnReadData, null);
                    break;
                }
                catch (IOException ex)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void OnShutDown(BaseMessage<ShutDownInfo> pObj)
        {
            if (Disconnected != null)
                Disconnected();
            Reconnect();
        }

        public event Action Connecting;
        public event Action Disconnected;
        public event Action Connected;

        public void Disconnect(int pConnection)
        {
            if (_pipe.IsConnected)
                UtilityMethods.WriteCommandResult(_pipe, new ConnectionStopCommand {Connection = pConnection});
            else
                Reconnect();
        }
    }
}