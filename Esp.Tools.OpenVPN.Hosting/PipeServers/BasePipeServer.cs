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
       
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;
        private Task _task;

        protected BasePipeServer(string pPipeName, int pMaxConnections)
        {
            _cancellationToken = _cancellationTokenSource.Token;
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

            _ = Task.Run(async () => await (_task = RunConnectLoop()));
        }

        private async Task RunConnectLoop()
        {
            try
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    await _pipeServer.WaitForConnectionAsync(_cancellationToken);

                    var connected = true;

                    await OnConnection();

                    try
                    {
                        await UtilityMethods.WriteCommandResultAsync(_pipeServer, new InitializedMessage(0),
                            _cancellationToken);
                    }
                    catch (InvalidOperationException ex)
                    {
                        EventLogHelper.LogEvent($"InvalidOperationException Writing To Pipe: " + ex.Message + "\n\r" +
                                                ex.StackTrace);
                        connected = false;
                    }

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
                            EventLogHelper.LogEvent($"IOException Reading From Pipe: {ex.Message}\n\r {ex.StackTrace}");
                        }
                        catch (InvalidOperationException ex)
                        {
                            connected = false;
                            EventLogHelper.LogEvent(
                                $"InvalidOperationException Reading From Pipe: {ex.Message}\n\r {ex.StackTrace}");
                        }
                    }

                    if (_pipeServer.IsConnected)
                        try
                        {
                            _pipeServer.Disconnect();
                        }
                        catch (Exception ex)
                        {
                        }
                
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            EventLogHelper.LogEvent($"OpenVPN Host Server shutting down pipe '{_pipeName}'");
        }

      



        protected async Task SendMessageAsync(IMessage pMessage)
        {
            try
            {
                if (_pipeServer.CanWrite && _pipeServer.IsConnected)
                    await UtilityMethods.WriteCommandResultAsync(_pipeServer, pMessage, _cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                EventLogHelper.LogEvent($"InvalidOperationException Writing To Pipe: {ex.Message}\n\r {ex.StackTrace}");
            }
        }

      
        protected abstract Task OnConnection();

        public async Task Shutdown()
        {
            if (_pipeServer.IsConnected)
            {
                try
                {
                    await SendMessageAsync(new ShutDownMessage());
                    await _pipeServer.FlushAsync();
                    _pipeServer.Disconnect();
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogEvent(
                        $"{ex.GetType().Name} Sending shutdown message to pipe: {ex.Message}\n\r {ex.StackTrace}");
                }
            }

            Task.WaitAll(_task, Task.Run(() =>
            {
                _cancellationTokenSource.Cancel();
            }));
        }
    }
        
       
}