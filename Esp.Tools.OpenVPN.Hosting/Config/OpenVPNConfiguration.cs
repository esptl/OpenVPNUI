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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.ConnectionFile;
using Esp.Tools.OpenVPN.EventLog;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages.Contracts;

namespace Esp.Tools.OpenVPN.Hosting.Config
{
    public class OpenVPNConfiguration
    {
        private readonly Regex _adaptersInUseRegex =
            new Regex(".*All .* adapters on this system are currently in use.*");

        private readonly ConnectionDefinitionFile _file;

        private readonly Regex _interfaceRegex =
            new Regex(".*device (?<intName>.*) opened: .*\\{(?<interface>.*)\\}.*");

        private readonly List<OutputLine> _output = new List<OutputLine>();
        private ConnectionError _error;
        private readonly byte[] _errorBuffer = new byte[4096];
        private IntPtr _event;
        private string _eventName;
        private string _genericError;
        private string _interface;
        private string _password;
        private Process _process;
        private ConnectionStatus _status;
        private string _username;


        public OpenVPNConfiguration(int pIndex, ConnectionDefinitionFile pFile)
        {
            _file = pFile;

            Index = pIndex;
            LoadConfigFile();
        }

        public ConnectionStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                if (StatusChanged != null)
                    StatusChanged(value);
            }
        }

        public string Interface
        {
            get => _interface;
            set
            {
                _interface = value;
                if (InterfaceChanged != null)
                    InterfaceChanged(value);
            }
        }

        public bool RequiresUsername { get; private set; }

        public string ConfigurationName => _file.ConnectionName;

        public ConfigurationInfo ConfigurationInfo => new ConfigurationInfo
        {
            ConnectionStatus = Status,
            Name = ConfigurationName,
            Interface = Interface,
            Error = _error,
            GenericErrorMessage = _genericError,
            RequiresUsername = RequiresUsername,
            AuthorityThumbPrint = _file.AuthorityCert.Thumbprint,
            ThumbPrint = _file.CertificateThumbPrint
        };

        public int Index { get; }

        public IEnumerable<OutputLine> Output => _output;

        private void LoadConfigFile()
        {
            RequiresUsername = _file.HasOption("auth-user-pass");
        }

        public void Connect()
        {
            if (_process == null)
            {
                _interface = null;
                _error = ConnectionError.None;
                _genericError = null;

                ///Status = ConnectionStatus.Connecting;

                _eventName = "vpn" + Guid.NewGuid();
                _event = CreateEvent(IntPtr.Zero, true, false, _eventName);
                _output.Clear();
                var workingPath = Configuration.Configuration.Current.WorkingPath + "openvpn\\bin\\";
                _process = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = Configuration.Configuration.Current.AppDataPath,
                        FileName = workingPath + "openvpn.exe",
                        Arguments = "--service " + _eventName + " 0 --config stdin",
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        ErrorDialog = false
                    }
                };

                _process.OutputDataReceived += OnOutputDataRecieved;
                _process.Exited += OnExited;


                _process.EnableRaisingEvents = true;
                _process.Start();

                var configText = _file.ComposedConfiguration;
                _process.StandardInput.WriteLine(configText);
                _process.StandardInput.Write((char) 26);
                _process.StandardInput.Flush();

                _process.StandardError.BaseStream.BeginRead(_errorBuffer, 0, _errorBuffer.Length, OnErrorData, null);
                _process.BeginOutputReadLine();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr pLpEventAttributes, bool pBManualReset, bool pBInitialState,
            [MarshalAs(UnmanagedType.LPStr)] string pLpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr pHObject);

        [DllImport("kernel32.dll")]
        private static extern bool SetEvent(IntPtr pHEvent);


        public void Disconnect(bool pKill)
        {
            if (Status == ConnectionStatus.Authenticating)
            {
                if (_process != null)
                {
                    _process.StandardInput.WriteLine();
                    _process.StandardInput.WriteLine();
                }
                Status = ConnectionStatus.Disconnecting;
            }
            if (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting ||
                Status == ConnectionStatus.Disconnecting && pKill)
                if (pKill)
                {
                    if (_process != null)
                        _process.Kill();
                    OnExited(this, new EventArgs());
                }
                else
                {
                    SetEvent(_event);

                    Status = ConnectionStatus.Disconnecting;
                }
        }

        private void OnErrorData(IAsyncResult pAr)
        {
            if (_process != null)
            {
                var read = _process.StandardError.BaseStream.EndRead(pAr);
                if (read != 0)
                {
                    var data = new byte[read];
                    Array.Copy(_errorBuffer, 0, data, 0, read);
                    var str = Encoding.ASCII.GetString(data);

                    if (str != null)
                        OnOutputRecieved(new OutputLine(OutputType.Error, str));
                    _process.StandardError.BaseStream.BeginRead(_errorBuffer, 0, _errorBuffer.Length, OnErrorData,
                        null);
                }
            }
        }

        private void OnExited(object pSender, EventArgs pE)
        {
            CloseHandle(_event);
            Interface = null;
            Status = ConnectionStatus.Disconnected;
            _process.Dispose();
            _process = null;
        }

        public event Action<OutputLine> OutputRecieved;
        public event Action<ConnectionStatus> StatusChanged;
        public event Action<string> InterfaceChanged;
        public event Action AuthInfoRequired;

        protected void OnOutputRecieved(OutputLine pLine)
        {
            if (pLine.Line != null && _process != null)
            {
                if (pLine.Line.Contains("Initialization Sequence Completed"))
                    Status = ConnectionStatus.Connected;

                if (pLine.Line.Contains("Exiting"))
                    Status = ConnectionStatus.Disconnected;

                if (pLine.Line.Contains("Cannot load certificate"))
                    _error = ConnectionError.Certificate;


                if (pLine.Line.Contains(" link remote") && Status == ConnectionStatus.Disconnected)
                    Status = ConnectionStatus.Connecting;

                if (_interfaceRegex.IsMatch(pLine.Line))
                {
                    var matches = _interfaceRegex.Matches(pLine.Line);
                    Interface = matches[0].Groups["interface"].Value;
                }

                if (pLine.Line.Contains("process restarting"))
                    Status = ConnectionStatus.Connecting;


                if (pLine.Line.StartsWith("Enter Auth Username:"))
                    SendAuthUsername();


                if (pLine.Line.StartsWith("Enter Auth Password:"))
                    SendAuthPassword();

                if (_adaptersInUseRegex.IsMatch(pLine.Line))
                    _error = ConnectionError.NoAvailableInterface;

                if (pLine.Line.Contains("AUTH_FAILED"))
                    _error = ConnectionError.Password;

                _output.Add(pLine);
                if (OutputRecieved != null)
                    OutputRecieved(pLine);
            }
        }

        private void SendAuthPassword()
        {
            _process?.StandardInput.WriteLine(_password);
            _password = null;
        }

        private void SendAuthUsername()
        {
            _process?.StandardInput.WriteLine(_username);
            _username = null;
        }

        private void OnOutputDataRecieved(object pSender, DataReceivedEventArgs pE)
        {
            OnOutputRecieved(new OutputLine(OutputType.Output, pE.Data));
        }

        public void SendAuthInfo(BaseMessage<AuthInfo> pInfo)
        {
            Status = ConnectionStatus.Connecting;
            _password = pInfo.Data.Password;
            _username = pInfo.Data.UserName;
        }

        public void ClearOutput()
        {
            _output.Clear();
        }

        public void SetCertificate(BaseMessage<SetConfigurationCertificateInfo> pRequest)
        {
            var certs = CertificateManager.Current.GetCertificates(_file.AuthorityCert);
            if (!certs.ToList().Exists(pX => pX.ThumbPrint == pRequest.Data.ThumbPrint))
            {
                EventLogHelper.LogEvent("Error finding certificate with thumbprint: " + pRequest.Data.ThumbPrint);
                return;
            }
            _file.CertificateThumbPrint = pRequest.Data.ThumbPrint;
            _file.Save();
        }

        public void Delete()
        {
            if (Status != ConnectionStatus.Disconnected && Status != ConnectionStatus.Disconnecting)
                Disconnect(false);
            _file.Delete();
            if (Deleted != null)
                Deleted(this);
        }

        public event Action<OpenVPNConfiguration> Deleted;
    }
}