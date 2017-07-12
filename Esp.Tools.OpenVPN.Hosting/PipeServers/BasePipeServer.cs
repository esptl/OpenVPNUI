using System;
using System.Collections.Generic;
using System.IO.Pipes;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages;
using Esp.Tools.OpenVPN.IPCProtocol.Messages;

namespace Esp.Tools.OpenVPN.Hosting.PipeServers
{
    public abstract class BasePipeServer
    {
        private const int BufferSize = 200000;

        private readonly string _pipeName;
        private readonly int _maxConnections;
        private IEnumerable<IMessageReader> _messageReaders;
        private readonly byte[] _buffer = new byte[BufferSize];

        private NamedPipeServerStream _pipeServer;
        private IAsyncResult _readAsync;

        protected BasePipeServer(string pPipeName, int pMaxConnections)
        {
            _pipeName = pPipeName;
            _maxConnections = pMaxConnections;
            Initialize();
        }

        private void Initialize()
        {
            _messageReaders = MessageReaders;

            var pipeSecurity = new PipeSecurity();
            
            foreach(var rule in PipeAccessRules)
                pipeSecurity.AddAccessRule(rule);

            _pipeServer = 
                new NamedPipeServerStream(
                    _pipeName, 
                    PipeDirection.InOut,
                     _maxConnections, 
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous,
                    BufferSize,
                    BufferSize, 
                    pipeSecurity);

            _pipeServer.BeginWaitForConnection(WaitForConnection, null);
        }

        protected abstract IEnumerable<PipeAccessRule> PipeAccessRules { get;  }
        protected abstract IEnumerable<IMessageReader> MessageReaders { get; } 

        private void WaitForRead(IAsyncResult pAr)
        {
            int read = _pipeServer.EndRead(_readAsync);
            if (read == 0)
            {
                _pipeServer.Disconnect();

                _pipeServer.BeginWaitForConnection(WaitForConnection, null);
                _readAsync = null;
            }
            else
            {
                var buf = new byte[read];
                Array.Copy(_buffer, 0, buf, 0, read);
                UtilityMethods.ReadMessage(buf,_messageReaders);
                _readAsync = _pipeServer.BeginRead(_buffer, 0, _buffer.Length, WaitForRead, null);
            }
        }

        protected void WaitForConnection(IAsyncResult pAr)
        {
            _pipeServer.EndWaitForConnection(pAr);
            OnConnection();
            
            UtilityMethods.WriteCommandResult(_pipeServer,new InitializedMessage(0));
            _readAsync = _pipeServer.BeginRead(_buffer, 0, _buffer.Length, WaitForRead, null);
        }


        protected void SendMessage(IMessage pMessage)
        {
            if (_pipeServer.CanWrite && _pipeServer.IsConnected)
                UtilityMethods.WriteCommandResult(_pipeServer,pMessage);
        }

        protected abstract void OnConnection();

        public void Shutdown()
        {
            SendMessage(new ShutDownMessage());
        }
    }
}
