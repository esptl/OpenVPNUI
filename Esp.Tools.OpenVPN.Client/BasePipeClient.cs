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
using System.Configuration;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly CancellationToken _cancellationToken = new CancellationToken(false);
        private readonly Task _task;
        private bool _connected = true;

        protected BasePipeClient(string pPipeName)
        {
            _pipeName = pPipeName;
            _task = Reconnect();
        }

        protected abstract IEnumerable<IMessageReader> MessageReaders { get; }

        protected void SendCommand(IMessage pMessage)
        {
            if (_pipe.CanWrite)
                UtilityMethods.WriteCommandResult(_pipe,
                    pMessage);
        }


        protected async Task SendCommandAsync(IMessage pMessage)
        {
            if (_pipe.CanWrite && _pipe.IsConnected)
                await UtilityMethods.WriteCommandResultAsync(_pipe, pMessage, _cancellationToken);
        }


        protected async Task Reconnect()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_messageTypes == null)
                {
                    var lst = MessageReaders.ToList();
                    lst.Add(new MessageReader<ShutDownInfo>(ShutDownMessage.MessageKey)
                        { MessageRecieved = OnShutDown });
                    _messageTypes = lst.ToArray();
                }

                _pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut,
                    PipeOptions.Asynchronous);

                Connecting?.Invoke();
                await _pipe.ConnectAsync(_cancellationToken);


                Connected?.Invoke();
                _connected = true;
                while (_connected)
                {
                    var read = await _pipe.ReadAsync(_buffer, 0, _buffer.Length, _cancellationToken);
                    if (read > 0)
                    {
                        var data = new byte[read];
                        Array.Copy(_buffer, 0, data, 0, read);
                        await UtilityMethods.ReadMessage(data, _messageTypes);
                    }
                    else
                        _connected = false;
                }
                Disconnected?.Invoke();
                await Task.Delay(500, _cancellationToken);
            }
        }
    

        private async Task OnShutDown(BaseMessage<ShutDownInfo> pObj)
        {
            if (_connected)
            {
                await _pipe.FlushAsync(_cancellationToken);
                _connected = false;

            }
        }

        public event Action Connecting;
        public event Action Disconnected;
        public event Action Connected;


    }
}