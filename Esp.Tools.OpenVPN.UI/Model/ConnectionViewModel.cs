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
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages.Contracts;
using Timer = System.Timers.Timer;

namespace Esp.Tools.OpenVPN.UI.Model
{
    public class ConnectionViewModel : ViewModelBase
    {
        private readonly ConnectionsViewModel _connections;
        private readonly ControllerPipeClient _pipeClient;
        private readonly Timer _timer;
        private string _bandwidthText;
        private string _dnsAddresses;
        private string _errorMessage;
        private bool _ignoreConnecting;
        private bool _insetLog;
        private string _interfaceId;
        private string _ipAddress;
        private bool _isAuthenticatingCanceled;
        private bool _isConnectingCanceled;
        private bool _isConnectingWithAuth;

        private long _lastIn, _lastOut;
        private string _password;
        private bool _showLog;
        private ConnectionStatus _status;
        private string _totalBandwidthText;
        private string _username;

        public ConnectionViewModel(ConnectionsViewModel pConnections, ControllerPipeClient pPipeClient,
            BaseMessage<ConfigurationInfo> pInfo)
        {
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimer;
            _connections = pConnections;
            _pipeClient = pPipeClient;
            Error = pInfo.Data.Error;

            Index = pInfo.Connection;
            InterfaceID = pInfo.Data.Interface;

            LoginCommand = new BasicCommand(OnLogin, () => !string.IsNullOrEmpty(Username) &&
                                                           !string.IsNullOrEmpty(Password));
            ClearLogCommand = new BasicCommand(OnClearLog);
            CopyLogCommand = new BasicCommand(OnCopyLog);
            Status = pInfo.Data.ConnectionStatus;

            if (Status != ConnectionStatus.Disconnected)
                _pipeClient.GetFullInfo(Index);
            Name = pInfo.Data.Name;
            ConnectCommand = new BasicCommand(
                pObj =>
                {
                    if (Status != ConnectionStatus.Disconnected)
                    {
                        switch (Status)
                        {
                            case ConnectionStatus.Authenticating:
                                IsAuthenticatingCanceled = true;
                                break;
                            case ConnectionStatus.Connecting:
                                IsConnectingCanceled = true;
                                _pipeClient.Disconnect(Index);
                                break;

                            case ConnectionStatus.Connected:
                                Status = ConnectionStatus.Disconnecting;
                                break;
                        }
                        ShowLog = false;
                    }
                    else
                    {
                        if (!ShowLog && _connections.AnyShowLog)
                            ShowLog = true;
                        if (pInfo.Data.RequiresUsername)
                        {
                            Status = ConnectionStatus.Authenticating;
                        }
                        else
                        {
                            Status = ConnectionStatus.Connecting;   
                            _pipeClient.Connect(Index);
                        }
                        _ignoreConnecting = true;
                    }
                    var thread = new Thread(
                        () =>
                        {
                            Thread.Sleep(300);
                            if (Status == ConnectionStatus.Connecting &&
                                _ignoreConnecting)
                            {
                            }
                            else
                            {
                                _pipeClient.Disconnect(pInfo.Connection);
                            }
                        });
                    thread.Priority = ThreadPriority.Lowest;

                    thread.Start();
                }, pObj => true);
        }

        public BasicCommand CopyLogCommand { get; }

        public BasicCommand ClearLogCommand { get; }

        public int Index { get; set; }
        public string Name { get; }

        public string TotalBandwidthText
        {
            get => _totalBandwidthText;
            set
            {
                _totalBandwidthText = value;
                OnPropertyChanged("TotalBandwidthText");
            }
        }

        public ConnectionStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    var oldStatus = _status;
                    _status = value;
                    OnPropertyChanged("Status");
                    OnPropertyChanged("ConnectButtonName");
                    switch (value)
                    {
                        case ConnectionStatus.Connected:
                            IsConnectingWithAuth = false;
                            OnConnected();
                            break;
                        case ConnectionStatus.Connecting:
                            if (oldStatus == ConnectionStatus.Authenticating)
                                IsConnectingWithAuth = true;
                            OnConnecting();
                            break;
                        case ConnectionStatus.Disconnected:
                            IsAuthenticatingCanceled = false;
                            IsConnectingCanceled = false;
                            IsConnectingWithAuth = false;
                            OnDisconnected();
                            break;
                        case ConnectionStatus.Authenticating:
                            Username = null;
                            Password = null;
                            break;
                    }
                    OnPropertyChanged("IsConnected");
                    OnPropertyChanged("IsAuthenticating");
                    OnPropertyChanged("IsConnecting");
                    OnPropertyChanged("IsDisconnected");
                    OnPropertyChanged("IsDisconnecting");
                    OnPropertyChanged("IsError");
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged("Username");

                LoginCommand.TriggerChanged();
            }
        }

        public bool IsConnectingCanceled
        {
            get => _isConnectingCanceled;
            set
            {
                _isConnectingCanceled = value;
                OnPropertyChanged("IsConnectingCanceled");
            }
        }

        public bool IsAuthenticatingCanceled
        {
            get => _isAuthenticatingCanceled;
            set
            {
                _isAuthenticatingCanceled = value;
                OnPropertyChanged("IsAuthenticatingCanceled");
                OnPropertyChanged("IsAuthenticating");
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged("Password");
                LoginCommand.TriggerChanged();
            }
        }

        public BasicCommand LoginCommand { get; set; }

        public ICommand ConnectCommand { get; set; }

        public string ConnectButtonName
        {
            get
            {
                switch (Status)
                {
                    case ConnectionStatus.Disconnecting:
                    case ConnectionStatus.Connecting:
                    case ConnectionStatus.Connected:
                        return "Disconnect";
                    default:
                        return "Connect";
                }
            }
        }

        public bool IsConnected => Status == ConnectionStatus.Connected;

        public bool IsAuthenticating => Status == ConnectionStatus.Authenticating && !IsAuthenticatingCanceled;

        public bool IsConnecting => Status == ConnectionStatus.Connecting && !IsConnectingWithAuth;

        public bool IsConnectingWithAuth
        {
            get => _isConnectingWithAuth;
            set
            {
                _isConnectingWithAuth = value;
                OnPropertyChanged("IsConnectingWithAuth");
            }
        }

        public bool IsDisconnected => Status == ConnectionStatus.Disconnected && Error == ConnectionError.None;

        public bool IsError => Status == ConnectionStatus.Disconnected && Error != ConnectionError.None;

        public ConnectionError Error { get; set; }

        public string ErrorMessage
        {
            get
            {
                switch (Error)
                {
                    case ConnectionError.None:
                        return "Success";
                    case ConnectionError.Password:
                        return "Invalid username or password.";
                    case ConnectionError.NoAvailableInterface:
                        return
                            "You have run out of virtual ethernet adapters. Please disconnect another VPN connection or add another virtual adapter.";
                    case ConnectionError.HostUncontactable:
                        return "Could not contact remote VPN server.";
                    case ConnectionError.Certificate:
                        return "Validation of client or server certificates failed.";
                }
                return "An error occurred while connecting.";
            }
        }

        public bool IsDisconnecting => Status == ConnectionStatus.Disconnecting && !IsConnectingCanceled &&
                                       !IsAuthenticatingCanceled;

        public string InterfaceID
        {
            get => _interfaceId;
            set
            {
                _interfaceId = value;
                NetworkInterface =
                    NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(pX => pX.Id == "{" + value + "}");

                // i[0].GetIPProperties(). 
                OnPropertyChanged("InterfaceName");
            }
        }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged("");
            }
        }

        public bool ShowLog
        {
            get { return _showLog; }
            set
            {
                if (!_insetLog)
                {
                    if (value && LogShown != null)
                        LogShown();
                    _insetLog = true;
                    _showLog = value;
                    OnPropertyChanged("ShowLog");
                    OnPropertyChanged("HideLog");
                    _insetLog = false;
                }
            }
        }

        public bool HideLog => !ShowLog;

        public NetworkInterface NetworkInterface { get; set; }

        public string Text { get; set; }

        public string DnsAddresses
        {
            get => _dnsAddresses;
            set
            {
                _dnsAddresses = value;
                OnPropertyChanged("DnsAddresses");
            }
        }

        public string BandwidthText
        {
            get => _bandwidthText;
            set
            {
                _bandwidthText = value;
                OnPropertyChanged("BandwidthText");
            }
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnLogin()
        {
            _pipeClient.SendUsernamePassword(Index, Username, Password);
            _pipeClient.Connect(Index);
            Username = null;
            Password = null;
        }

        private void OnClearLog()
        {
            Text = "";
            OnPropertyChanged("Text");
            _pipeClient.ClearLog(Index);
        }

        private void OnCopyLog()
        {
            var keepGoing = true;
            while (keepGoing)
                try
                {
                    var txt = Text.Replace("\n", "\r\n");
                    Clipboard.SetText(txt);
                    keepGoing = false;
                }
                catch (Exception)
                {
                    Thread.Sleep(50);
                }
        }

        private string ConvertAmount(long pAmount)
        {
            var cur = pAmount;
            var multiple = 1;
            var index = 0;
            while (cur > 1024)
            {
                cur /= 1024;
                multiple *= 1024;
                index++;
            }
            var amt = (decimal) pAmount;
            switch (index)
            {
                case 0: // bytes
                    return pAmount + "b";
                case 1: // k
                    return string.Format("{0:0.00}k", amt / multiple);
                case 2: // mb
                    return string.Format("{0:0.00}mb", amt / multiple);
                default: // gb
                    return string.Format("{0:0.00}gb", amt / multiple);
            }
        }

        private void OnTimer(object pSender, ElapsedEventArgs pE)
        {
            if (NetworkInterface != null)
            {
                var stats = NetworkInterface.GetIPv4Statistics();
                var totalIn = stats.BytesReceived - _lastIn;
                var totalOut = stats.BytesSent - _lastOut;
                BandwidthText = string.Format("{0}s/{1}s",
                    new object[] {ConvertAmount(totalIn), ConvertAmount(totalOut)});
                TotalBandwidthText = string.Format("{0}/{1}", ConvertAmount(stats.BytesReceived),
                    ConvertAmount(stats.BytesSent));
                _lastIn = stats.BytesReceived;
                _lastOut = stats.BytesSent;
            }
        }

        public event Action LogShown;

        private void OnConnecting()
        {
            if (Connecting != null)
                Connecting();
        }

        public event Action Connected;
        public event Action Disconnected;
        public event Action Connecting;

        private void OnConnected()
        {
            if (NetworkInterface != null)
            {
                var ip = NetworkInterface.GetIPProperties();
                IpAddress =
                    ip.UnicastAddresses.Where(pX => pX.Address.AddressFamily != AddressFamily.InterNetworkV6)
                        .Aggregate(
                            "",
                            (pAccum, pItem) =>
                                pAccum +
                                pItem.Address.ToString() + " ");
                DnsAddresses =
                    ip.DnsAddresses.Where(pX => pX.AddressFamily != AddressFamily.InterNetworkV6)
                        .Aggregate("",
                            (pAccum,
                                    pItem) =>
                                    pAccum +
                                    pItem.ToString
                                        () + " ");

                var stats = NetworkInterface.GetIPv4Statistics();
                _lastIn = stats.BytesReceived;
                _lastOut = stats.BytesSent;
                _timer.Enabled = true;
                var x = stats.BytesReceived;
            }
            if (Connected != null)
                Connected();
        }

        private void OnDisconnected()
        {
            _timer.Enabled = false;
            IpAddress = null;
            DnsAddresses = null;
            if (Disconnected != null)
                Disconnected();
        }

        public void OnConnectInfo(BaseMessage<ConfigurationInfo> pInfo)
        {
            Error = pInfo.Data.Error;
            Status = pInfo.Data.ConnectionStatus;
            InterfaceID = pInfo.Data.Interface;
        }

        public void OnMessage(BaseMessage<OutputLine> pInfo)
        {
            if (Text == null)
                Text = pInfo.Data.Line;
            else
                Text += "\n" + pInfo.Data.Line;
            OnPropertyChanged("Text");
        }

        public void OnRequestPassword(RequestPasswordInfo pData)
        {
        }
    }
}