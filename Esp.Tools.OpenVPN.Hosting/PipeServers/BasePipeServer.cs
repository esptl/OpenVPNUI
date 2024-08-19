using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Esp.Tools.OpenVPN.EventLog;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages;
using Esp.Tools.OpenVPN.IPCProtocol.Messages;

namespace Esp.Tools.OpenVPN.Hosting.PipeServers
{
    public abstract class BasePipeServer
    {
        private const int BufferSize = 200000;
        private readonly byte[] _buffer = new byte[BufferSize];
        private readonly int _maxConnections;

        private readonly string _pipeName;
        private IEnumerable<IMessageReader> _messageReaders;

        private NamedPipeServerStream _pipeServer;
        private IAsyncResult _readAsync;
        private readonly CancellationToken _cancellationToken = new CancellationToken(false);
        private Task _task;

        protected BasePipeServer(string pPipeName, int pMaxConnections)
        {
            _pipeName = pPipeName;
            _maxConnections = pMaxConnections;
            Initialize();
        }

        protected abstract IEnumerable<PipeAccessRule> PipeAccessRules { get; }
        protected abstract IEnumerable<IMessageReader> MessageReaders { get; }

        private void Initialize()
        {
            _messageReaders = MessageReaders;

            var pipeSecurity = new PipeSecurity();

            foreach (var rule in PipeAccessRules)
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


            _task = RunConnectLoop();


        }

        private async Task RunConnectLoop()
        {
            while (true)
            {
                await _pipeServer.WaitForConnectionAsync(_cancellationToken);

                var connected = true;

                await OnConnection();

                await UtilityMethods.WriteCommandResultAsync(_pipeServer, new InitializedMessage(0), _cancellationToken);

                while (connected)
                {
                    try
                    {
                        var bytesRead = await _pipeServer.ReadAsync(_buffer, 0, _buffer.Length, _cancellationToken);
                        if (bytesRead == 0)
                            connected = false;
                        else
                        {
                            var buf = new byte[bytesRead];
                            Array.Copy(_buffer, 0, buf, 0, bytesRead);
                            await UtilityMethods.ReadMessage(buf, _messageReaders);
                        }
                    }
                    catch (IOException ex)
                    {
                        connected = false;
                        EventLogHelper.LogEvent($"IOException Reading From Pipe: " + ex.Message + "\n\r" + ex.StackTrace);
                    }
                }
                _pipeServer.Disconnect();
            }
        }

      



        protected async Task SendMessageAsync(IMessage pMessage)
        {
           if (_pipeServer.CanWrite && _pipeServer.IsConnected)
              await UtilityMethods.WriteCommandResultAsync(_pipeServer, pMessage, _cancellationToken);
        }

      
        protected abstract Task OnConnection();

        public void Shutdown()
        {
            SendMessageAsync(new ShutDownMessage());
        }
    }
}